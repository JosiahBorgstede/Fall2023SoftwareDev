using Newtonsoft.Json;

namespace Q.Common;
/// <summary>
/// An action that can be performed within a game
/// </summary>
[JsonConverter(typeof(IActionConverter))]
public interface IAction
{
    /// <summary>
    /// Determines if this action is valid.
    /// </summary>
    /// <param name="gameInfo">The public game info</param>
    /// <returns>True if this move is valid</returns>
    public bool IsValid(PlayerTurnInfo gameInfo);

    /// <summary>
    /// Gets the score of this action
    /// </summary>
    /// <param name="settings">Parameterizable game settings</param>
    /// <param name="gameInfo">The public game info</param>
    /// <returns>The score the player would earn for this action</returns>
    public int GetScore(ScoreConfiguration settings, PlayerTurnInfo gameInfo);

    /// <summary>
    /// Commits this move on the game state, transforming it. Returns the score
    /// delta for the active player.
    /// **WILL MUTATE STATE**
    /// </summary>
    /// <param name="state">The actual current game state</param>
    /// <return>The delta; new tiles given to the player</return>
    public IEnumerable<Tile> Commit(IGameState state);
}

/// A Pass action, which does nothing.
public class Pass : IAction
{
    public int GetScore(ScoreConfiguration settings, PlayerTurnInfo gameInfo)
    {
        return 0;
    }

    public bool IsValid(PlayerTurnInfo state) { return true; }

    public IEnumerable<Tile> Commit(IGameState state)
    {
        return new Tile[] {};
    }
}

/// An Exchange action, which exhcanges all of the player's tiles with new
/// tiles from the referee. Only valid if the player has the same number of
/// tiles as the referee or less.
public class Exchange : IAction
{
    public bool IsValid(PlayerTurnInfo state)
    {
        return state.CurrentPlayer.GetTiles().Count() <= state.RemainingTiles;
    }

    public int GetScore(ScoreConfiguration settings, PlayerTurnInfo gameInfo)
    {
        return 0;
    }

    public IEnumerable<Tile> Commit(IGameState state)
    {
        PlayerState player = state.ActivePlayer();
        state.AddTiles(player.GetTiles());
        return state.ReplaceActivePlayerTiles(new List<Tile>(player.GetTiles()));
    }
}

/// A Place action, which places the given tiles on the board according to
/// the rules of the Q game. The player obviously must actually possess
/// the tiles.
public class Place : IAction
{
    public List<Placement> Placements { get; }

    public Place(IEnumerable<Placement> placements)
    {
        Placements = new List<Placement>(placements);
        if (Placements.Count == 0)
        {
            throw new ArgumentException("Must have one placement");
        }
    }

    public Place(Placement placement) : this(new List<Placement>(){placement})
    {
    }

    public bool IsValid(PlayerTurnInfo state)
    {
        return PlayerHasTiles(state.CurrentPlayer)
            && (PlacementsSameRow() || PlacementsSameColumn())
            && PlacementFitsMap(state);
    }

    //FIXME: this assumes that the placement has not happened, is that what we
    //want?
    public int GetScore(ScoreConfiguration settings, PlayerTurnInfo gameInfo)
    {
        Map map = gameInfo.Map.PlaceMultiple(Placements);
        return ComputeScore(settings, map, gameInfo.CurrentPlayer);
    }

    public IEnumerable<Tile> Commit(IGameState state)
    {
        PlayerState player = state.ActivePlayer();
        state.Map = state.Map.PlaceMultiple(Placements);
        if (Placements.Count == player.GetTiles().Count())
        {
            state.IsGameOver = true;
        }
        return state.ReplaceActivePlayerTiles(Placements.Select(p => p.Tile));
    }

    private bool PlayerHasTiles(PlayerState player)
    {
        List<Tile> tiles = new List<Tile>(player.GetTiles());
        foreach (var placement in Placements)
        {
            if (!tiles.Remove(placement.Tile))
            {
                return false;
            }
        }
        return true;
    }

    private bool PlacementsSameRow()
    {
        var first = Placements[0];
        return Placements
            .Select(p => p.Coordinate.Y)
            .All(y => y == first.Coordinate.Y);
    }

    private bool PlacementsSameColumn()
    {
        var first = Placements[0];
        return Placements
            .Select(p => p.Coordinate.X)
            .All(x => x == first.Coordinate.X);
    }

    private bool PlacementFitsMap(PlayerTurnInfo state)
    {
        var map = state.Map;
        foreach (var placement in Placements)
        {
            if (map.ValidPlacement(placement))
            {
                map = map.PlaceTile(placement.Coordinate, placement.Tile);
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private int ComputeScore(ScoreConfiguration settings, Map map, PlayerState player)
    {
        int delta = ComputeScoreWithoutPlayer(settings, map);
        if (player.GetTiles().Count() == Placements.Count)
        {
            delta += settings.WinBonus;
        }
        return delta;
    }

    public int ComputeScoreWithoutPlayer(ScoreConfiguration settings, Map map)
    {
        return Placements.Count
            + ScoreContains(map)
            + ScoreQCompletions(settings, map);
    }

    private int ScoreContains(Map map)
    {
        return CollectRows(map).SelectMany(seq => seq).Count()
            + CollectCols(map).SelectMany(seq => seq).Count();
    }

    private IEnumerable<IEnumerable<Placement>> CollectRows(Map map)
    {
        return Placements
            .Select(map.GetConnectedRow)
            .Where(seq => seq.Count() > 1)
            .Distinct(new PlacementEnumerableComparer());
    }

    private IEnumerable<IEnumerable<Placement>> CollectCols(Map map)
    {
        return Placements
            .Select(map.GetConnectedCol)
            .Where(seq => seq.Count() > 1)
            .Distinct(new PlacementEnumerableComparer());
    }

    private int ScoreQCompletions(ScoreConfiguration settings, Map map)
    {
        return settings.QBonus * CollectQCompletions(map).Count();
    }

    private IEnumerable<IEnumerable<Placement>> CollectQCompletions(Map map)
    {
        return CollectRows(map)
            .Concat(CollectCols(map))
            .Where(seq => IsQCompletion(seq.Select(p => p.Tile)));
    }

    private bool IsQCompletion(IEnumerable<Tile> tiles)
    {
        return EnumerableUnorderedEqual(tiles.Select(t => t.Color),
                                        Enum.GetValues<Color>())
            || EnumerableUnorderedEqual(tiles.Select(t => t.Shape),
                                        Enum.GetValues<Shape>());
    }

    private bool EnumerableUnorderedEqual<T>(IEnumerable<T> e1, IEnumerable<T> e2)
    {
        return e1.Order().SequenceEqual(e2.Order());
    }
}

public class IActionConverter : JsonConverter<IAction>
{
    public override IAction? ReadJson(JsonReader reader,
                                      Type objectType,
                                      IAction? existingValue,
                                      bool hasExistingValue,
                                      JsonSerializer serializer)
    {
        if(reader.TokenType == JsonToken.String)
        {
            if((string) reader.Value == "pass")
            {
                return new Pass();
            }
            else if((string) reader.Value == "exchange")
            {
                return new Exchange();
            }
        }
        else if(reader.TokenType == JsonToken.StartArray)
        {
            var placements = serializer.Deserialize<List<Placement>>(reader);
            return new Place(placements);
        }
        throw new JsonReaderException("Unable to read in IAction");
    }

    public override void WriteJson(JsonWriter writer,
                                   IAction? value,
                                   JsonSerializer serializer)
    {
        switch (value)
        {
            case Pass:
                writer.WriteValue("pass");
                break;
            case Exchange:
                writer.WriteValue("exchange");
                break;
            case Place place:
                serializer.Serialize(writer, place.Placements);
                break;
            default:
                throw new JsonWriterException("Unable to write value: " + value);
        }
    }
}


