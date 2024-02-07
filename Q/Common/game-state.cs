using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace Q.Common;

public interface IGameState : IRenderable
{
    public Map Map { get; set; }

    public PlayerState ActivePlayer();

    public bool IsGameOver {get; set;}

    public Queue<Tile> Tiles {get;}
    public PlayerTurnInfo PublicInfo();

    public ICycle<PlayerState> Cycle();

    public void KickPlayer();

    public void AddTiles(IEnumerable<Tile> toAdd);

    public void BumpPlayer();

    //determines if the map has been updated during the current round
    public bool WasMapUpdated();

    public void NextRound();

    public int PassExchangeCounter {get; set;}

    public IEnumerable<Tile> ReplaceActivePlayerTiles(IEnumerable<Tile> toTake);

    void ReplaceCycle(ICycle<PlayerState> cycle);
}
/// The state of the Q game (i.e. everything that the referee can see).
/// Includes the map, the players and their turn order, which player is
/// active, and the deck of cards.
[JsonConverter(typeof(GameStateConverter))]
public class GameState :IGameState
{
    private Map _map;
    public Map Map
    {
        get {return _map;}
        set {_mapUpdated = true; _map = value;}
    }

    private bool _mapUpdated;
    private ICycle<PlayerState> _cycle;
    public Queue<Tile> Tiles {get;}
    public int PassExchangeCounter {get; set;}
    public bool IsGameOver {get; set;} = false;

    /// A Builder contains methods for modifying the settings, such as
    /// players, of a GameState, before constructing a GameState.
    public class Builder
    {
        private Queue<PlayerState> _players;
        private Queue<Tile> _tiles;

        /// Creates a builder with a ready-made sequence of tiles.
        public Builder()
        {
            _players = new Queue<PlayerState>();
            _tiles = new Queue<Tile>();
            foreach (var shape in Enum.GetValues<Shape>())
            {
                foreach (var color in Enum.GetValues<Color>())
                {
                    for (var i = 0; i < 30; ++i)
                    {
                        _tiles.Enqueue(new Tile(color, shape));
                    }
                }
            }
        }

        public Builder(int countPerTile)
        {
            _players = new Queue<PlayerState>();
            _tiles = new Queue<Tile>();
            foreach (var shape in Enum.GetValues<Shape>())
            {
                foreach (var color in Enum.GetValues<Color>())
                {
                    for (var i = 0; i < countPerTile; ++i)
                    {
                        _tiles.Enqueue(new Tile(color, shape));
                    }
                }
            }
        }

        /// Creates a builder with the given sequence of tiles
        public Builder(IEnumerable<Tile> tiles)
        {
            _players = new Queue<PlayerState>();
            _tiles = new Queue<Tile>(tiles);
        }

        /// Randomizes the sequence of tiles
        public Builder RandomizeTiles()
        {
            Random random = new Random();
            _tiles = new Queue<Tile>(_tiles.OrderBy(item => random.Next()));
            return this;
        }

        /// Adds a player to the pending game.
        public Builder AddPlayer(string name)
        {
            var tiles = new List<Tile>();
            for (var i = 0; i < 6; ++i)
            {
                tiles.Add(_tiles.Dequeue());
            }
            _players.Enqueue(new PlayerState(tiles, name));
            return this;
        }

        /// Constructs an initial GameState with the first tile at (0, 0).
        /// Throws an InvalidOperationException if the number of players is
        /// not in the range 2-4 inclusive.
        public GameState Build()
        {
            var tile = _tiles.Dequeue();
            return Build(new Map(tile));
        }

        /// Constructs an initial GameState with the given map.
        /// Throws an InvalidOperationException if the number of players is
        /// not in the range 2-4 inclusive.
        public GameState Build(Map map)
        {
            if (_players.Count < 2 || _players.Count > 4)
            {
                throw new InvalidOperationException("Player count must be 2-4");
            }
            return new GameState(
                map, new Cycle<PlayerState>(_players), _tiles.ToList());
        }
    }

    /// Constructs a GameState from a map and a list of players in turn order.
    /// The first player is at position 0. Throws an ArgumentException if the
    /// number of players is not in the range 2-4 inclusive.
    public GameState(Map map, ICycle<PlayerState> players, IEnumerable<Tile> tiles)
    {
        Tiles = new Queue<Tile>(tiles);
        Map = map;
        if (players.Count() < 2 || players.Count() > 4)
        {
            throw new ArgumentException("Game should have between 2-4 players");
        }
        _cycle = players;
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="state">state to copy</param>
    public GameState(IGameState state)
    {
        Tiles = new Queue<Tile>(state.Tiles);
        Map = state.Map;
        List<PlayerState> players = new();
        foreach(var player in state.Cycle().AllPlayers())
        {
            players.Add(new PlayerState(player));
        }
        _cycle = new Cycle<PlayerState>(players);

    }

    /// Returns info about a game publicly available to all players and
    /// observers.
    public PlayerTurnInfo PublicInfo()
    {
        var players = new List<PublicPlayerInfo>();
        var otherPlayers = from player in _cycle.OtherPlayers()
                           select player.PublicInfo();
        return new PlayerTurnInfo(Map,
                                  new PlayerState(ActivePlayer()),
                                  otherPlayers,
                                  Tiles.Count);
    }

    public ICycle<PlayerState> Cycle()
    {
        return _cycle;
    }

    public PlayerState ActivePlayer()
    {
        return _cycle.ActivePlayer();
    }

    /// Advances the turn to the next player
    public void BumpPlayer()
    {
        _cycle.BumpPlayer();
    }

    //determines if the map has been updated during the current round
    public bool WasMapUpdated()
    {
        return _mapUpdated;
    }

    public void NextRound()
    {
        _mapUpdated = false;
    }
    /// Advances the turn to the next player, while removing the active player
    /// Returns whether there are players left.
    public void KickPlayer()
    {
        AddTiles(_cycle.ActivePlayer().GetTiles());
        _cycle.KickPlayer();
    }

    /// Adds the tiles to the game state's deck
    public void AddTiles(IEnumerable<Tile> toAdd)
    {
        foreach(var tile in toAdd)
        {
            Tiles.Enqueue(tile);
        }
    }

    /// Draws N tiles from the front of the referee's deck
    private IEnumerable<Tile> DrawTiles(int n)
    {
        List<Tile> toGive = new();
        int min = Math.Min(n, Tiles.Count);
        for(int i = 0; i < min; i++)
        {
            toGive.Add(Tiles.Dequeue());
        }
        return toGive;
    }

    /// Replaces the given tiles possessed by the player with the same number
    /// of tiles from the game-state referee's deck, or as many tiles that the
    /// referee has left.
    public IEnumerable<Tile> ReplaceActivePlayerTiles(
        IEnumerable<Tile> toTake)
    {
        ActivePlayer().RemoveTiles(new List<Tile>(toTake));
        var tiles = DrawTiles(toTake.Count());
        ActivePlayer().AddTiles(new List<Tile>(tiles));
        return tiles;
    }

    /// <summary>
    /// Draws the current board state
    /// The top shows the scores of all the players, in turn order, with the
    /// current player being first
    /// Below that is the map
    /// Below that is the current players hand
    /// </summary>
    /// <param name="width">The width to fit the</param>
    /// <param name="height"></param>
    /// <returns></returns>
    public SKSurface Render(int width, int height)
    {
        SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
        SKCanvas canvas = surface.Canvas;
        int padding = Math.Min(Math.Min(width/10, height/10), 20);
        DrawPlayers(canvas, width / 2, 4 * height / 5, width / 2);
        MapDraw(canvas, padding, width / 2, 4 * height / 5);
        RenderNextTiles(canvas, width, height / 5, 4 * height / 5);
        return surface;
    }

    private void RenderNextTiles(SKCanvas canvas, int width, int height, int yOffset)
    {
        using SKPaint numPaint = new() {Color = SKColors.Black};
        numPaint.TextSize = height * 0.18f;
        numPaint.TextAlign = SKTextAlign.Center;
        canvas.DrawText("Remaining Tiles:" + Tiles.Count.ToString(),
                        width / 2,
                        height * 0.18f + yOffset,
                        numPaint);
        int tilesize = (int)Math.Min(width/6, height * 0.8f);
        int count = Math.Min(Tiles.Count(), 6);
        for(int i = 0; i < count; i++)
        {
            canvas.DrawSurface(Tiles.ElementAt(i).Render(tilesize, tilesize),
                               tilesize * i,
                               yOffset + (int)(0.2 * height));
        }
    }

    private void MapDraw(SKCanvas canvas, int padding, int width, int height)
    {
        SKSurface map = Map.Render(width - 2 * padding, height - 2 * padding);
        canvas.DrawSurface(map, padding, padding);
    }

    private void DrawPlayers(SKCanvas canvas, int width, int height, int xOffset)
    {
        int playerHeight = height / (_cycle.OtherPlayers().Count() + 1);
        if(ActivePlayer() != null)
        {
            canvas.DrawSurface(ActivePlayer().Render(width, playerHeight), xOffset, 0);
        }
        int playerCount = 1;
        foreach (var player in _cycle.OtherPlayers())
        {
            var playerSurf = player.Render(width, playerHeight);
            canvas.DrawSurface(playerSurf, xOffset, playerHeight * playerCount);
            playerCount++;
        }
    }

    public void ReplaceCycle(ICycle<PlayerState> cycle)
    {
        _cycle = cycle;
    }
}

public class GameStateConverter : JsonConverter<GameState>
{
    public override void WriteJson(
        JsonWriter writer,
        GameState value,
        JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("map");
        serializer.Serialize(writer, value.Map);
        writer.WritePropertyName("tile*");
        serializer.Serialize(writer, value.Tiles);
        writer.WritePropertyName("players");
        serializer.Serialize(writer, value.Cycle().AllPlayers());
        writer.WriteEndObject();
        writer.Flush();
    }

    public override bool CanWrite
    {
        get { return true; }
    }

    public override GameState ReadJson(
        JsonReader reader,
        Type objectType,
        GameState existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        JObject state = JObject.Load(reader);
        Map map = state["map"]!.ToObject<Map>();
        JArray jsonTiles = (JArray)state["tile*"];
        List<Tile> tiles = new List<Tile>();
        foreach (var json in jsonTiles) {
            tiles.Add(json.ToObject<Tile>());
        }
        JArray jsonPlayers = (JArray)state["players"];

        List<PlayerState> players = new List<PlayerState>();
        foreach (var json in jsonPlayers) {
            PlayerState player = json.ToObject<PlayerState>();
            players.Add(player);
        }

        var cycle = new Cycle<PlayerState>(players);

        return new GameState(map, cycle, tiles);
    }
}
