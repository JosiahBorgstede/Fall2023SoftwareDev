using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Q.Common;
namespace Q.Player;

[JsonConverter(typeof(IPlayerConverter))]
public interface IPlayer
{

    public bool GameOver { get; }

    public bool Won { get; }

    public IStrategy _strat {get;}
    //Gets the name of the player. This method should not throw during normal
    //execution.
    public string name();

    // the player is handed the inital map, which is visible to all,
    // plus an initial set of tiles
    // can throw
    public void setup(PlayerTurnInfo info, IEnumerable<Tile> st);

    //requests for the player to take a turn.
    public IAction takeTurn(PlayerTurnInfo p);

    // the player is handed a new set of tiles
    public void newTiles(IEnumerable<Tile> st);

    // the player is informed whether it won or not
    public void win(bool w);
}

public class InHousePlayer : IPlayer
{
    protected string _name;

    public bool GameOver {get; private set;}

    public bool Won {get; private set;}
    public IStrategy _strat {get; protected set;}

    public InHousePlayer(string name, IStrategy strategy)
    {
        GameOver = false;
        _name = name;
        _strat = strategy;
    }
    public virtual void newTiles(IEnumerable<Tile> st)
    {}

    public virtual void setup(PlayerTurnInfo info, IEnumerable<Tile> st)
    {}

    public virtual IAction takeTurn(PlayerTurnInfo p)
    {
        return _strat.ApplyStrategy(p);
    }

    public virtual void win(bool w)
    {
        GameOver = true;
        Won = w;
        return;
    }

    public string name()
    {
        return _name;
    }
}

public class ExceptionalPlayer : IPlayer
{
    private IPlayer _inner;
    public bool BreakNewTiles = false;
    public bool BreakSetup = false;
    public bool BreakTakeTurn = false;
    public bool BreakWin = false;

    public IStrategy _strat {get {return _inner._strat;}}

    public bool GameOver => false;

    public bool Won => false;

    public ExceptionalPlayer(IPlayer inner)
    {
        _inner = inner;
    }

    public void newTiles(IEnumerable<Tile> st)
    {
        if (BreakNewTiles)
        {
            throw new Exception("Broken newTiles");
        }
        _inner.newTiles(st);
    }

    public void setup(PlayerTurnInfo info, IEnumerable<Tile> st)
    {
        if (BreakSetup)
        {
            throw new Exception("Broken setup");
        }
        _inner.setup(info, st);
    }

    public IAction takeTurn(PlayerTurnInfo p)
    {
        if (BreakTakeTurn)
        {
            throw new Exception("Broken setup");
        }
        return _inner.takeTurn(p);
    }

    public void win(bool w)
    {
        if (BreakWin)
        {
            throw new Exception("Broken win");
        }
        _inner.win(w);
    }

    public string name()
    {
        return _inner.name();
    }
}

public abstract class CheatingPlayer : InHousePlayer
{
    public CheatingPlayer(string name, IStrategy strat) : base(name, strat)
    {
    }
    public abstract override IAction takeTurn(PlayerTurnInfo p);
}

public class NonLineCheatPlayer : CheatingPlayer
{
    public NonLineCheatPlayer(string name, IStrategy strat) : base(name, strat)
    { }

    public override IAction takeTurn(PlayerTurnInfo p)
    {
        var result = _strat.ApplyStrategy(p);
        if (result.GetType() != typeof(Place)) { return result; }
        var placeAct = result as Place;
        var firstPlacement = placeAct.Placements.First();
        var otherTiles = p.CurrentPlayer.GetTiles().ToList();
        otherTiles.Remove(firstPlacement.Tile);
        foreach (var tile in otherTiles)
        {
            IEnumerable<Coordinate> coordinates =
            p.Map.ValidPlacements(tile)
                .Where(t => t.X != firstPlacement.Coordinate.X
                         && t.Y != firstPlacement.Coordinate.Y);

            if (coordinates.Any())
            {
                Placement secondPlacement = new(coordinates.First(), tile);
                return new Place(new List<Placement>() { firstPlacement, secondPlacement });
            }
        }
        return _strat.ApplyStrategy(p);
    }
}

public class NonAdjCheatPlayer : CheatingPlayer
{
    public NonAdjCheatPlayer(string name, IStrategy strat) : base(name, strat)
    {}

    public override IAction takeTurn(PlayerTurnInfo p)
    {
        var tile = p.CurrentPlayer.Tiles.First();
        var bounds = p.Map.GetBounds();
        Placement outOfBounds = new(new(bounds.MaxX + 2, bounds.MaxY + 2), tile);
        return new Place(new List<Placement>(){outOfBounds});
    }
}

public class NotOwnedCheatPlayer : CheatingPlayer
{
    public NotOwnedCheatPlayer(string name, IStrategy strat) : base(name, strat)
    { }

    public override IAction takeTurn(PlayerTurnInfo p)
    {
        foreach (var shape in Enum.GetValues<Shape>())
        {
            foreach (var color in Enum.GetValues<Color>())
            {
                Tile tile = new Tile(color, shape);
                if (!p.CurrentPlayer.Tiles.Contains(tile))
                {
                    if (p.Map.ValidPlacements(tile).Any())
                    {
                        Placement placement = new(p.Map.ValidPlacements(tile).First(), tile);
                        return new Place(new List<Placement>() { placement });
                    }
                }
            }
        }
        return _strat.ApplyStrategy(p);
    }
}

public class BadAskCheatPlayer : CheatingPlayer
{
    public BadAskCheatPlayer(string name, IStrategy strat) : base(name, strat)
    {}

    public override IAction takeTurn(PlayerTurnInfo p)
    {
        if(p.CurrentPlayer.Tiles.Count > p.RemainingTiles)
        {
            return new Exchange();
        }
        else
        {
            return _strat.ApplyStrategy(p);
        }
    }
}

public class NoFitCheatPlayer : CheatingPlayer
{
    public NoFitCheatPlayer(string name, IStrategy strat) : base(name, strat)
    {}

    public override IAction takeTurn(PlayerTurnInfo p)
    {
        foreach(var tile in p.CurrentPlayer.GetTiles())
        {
            foreach(var coord in p.Map.OpenSpots())
            {
                Placement currentPlacement = new(coord, tile);
                if(!p.Map.ValidPlacement(currentPlacement))
                {
                    return new Place(new List<Placement>(){currentPlacement});
                }
            }
        }
        return _strat.ApplyStrategy(p);
    }
}

public abstract class BlockingPlayer : InHousePlayer
{
    public int CountUntilBreak;
    public BlockingPlayer(string name, IStrategy strategy, int countUntilBreak)
          : base(name, strategy)
    {
        CountUntilBreak = countUntilBreak;
    }

    protected void CheckToBreak()
    {
        CountUntilBreak--;
        if(0 == CountUntilBreak) { while(true); }
    }
}

public class BlockNewTiles : BlockingPlayer
{
    public BlockNewTiles(string name, IStrategy strategy, int countUntilBreak)
          : base(name, strategy, countUntilBreak)
    {}

    public override void newTiles(IEnumerable<Tile> st)
    {
        CheckToBreak();
        base.newTiles(st);
    }
}

public class BlockWin : BlockingPlayer
{
    public BlockWin(string name, IStrategy strategy, int countUntilBreak)
          : base(name, strategy, countUntilBreak)
    {}

    public override void win(bool w)
    {
        CheckToBreak();
        base.win(w);
    }
}

public class BlockSetup : BlockingPlayer
{
    public BlockSetup(string name, IStrategy strategy, int countUntilBreak)
          : base(name, strategy, countUntilBreak)
    {}

    public override void setup(PlayerTurnInfo info, IEnumerable<Tile> st)
    {
        CheckToBreak();
        base.setup(info, st);
    }
}

public class BlockTakeTurn : BlockingPlayer
{
    public BlockTakeTurn(string name, IStrategy strategy, int countUntilBreak)
          : base(name, strategy, countUntilBreak)
    {}

    public override IAction takeTurn(PlayerTurnInfo p)
    {
        CheckToBreak();
        return base.takeTurn(p);
    }
}

public class IPlayerConverter : JsonConverter<IPlayer>
{
    public override void WriteJson(
        JsonWriter writer,
        IPlayer value,
        JsonSerializer serializer)
    {
        writer.WriteStartArray();
        writer.WriteValue(value.name());
        switch (value._strat)
        {
            case Dag:
                writer.WriteValue("dag");
                break;
            case Ldasg:
                writer.WriteValue("ldasg");
                break;
        }
        switch(value)
        {
            case ExceptionalPlayer exnPlay:
                if(exnPlay.BreakNewTiles) {writer.WriteValue("new-tiles");}
                else if(exnPlay.BreakSetup) {writer.WriteValue("setup");}
                else if(exnPlay.BreakTakeTurn){writer.WriteValue("take-turn");}
                else if(exnPlay.BreakWin){writer.WriteValue("win");}
                break;
            case NonAdjCheatPlayer:
                writer.WriteValue("a cheat");
                writer.WriteValue("non-adjacent-coordinate");
                break;
            case NotOwnedCheatPlayer:
                writer.WriteValue("a cheat");
                writer.WriteValue("tile-not-owned");
                break;
            case NonLineCheatPlayer:
                writer.WriteValue("a cheat");
                writer.WriteValue("not-a-line");
                break;
            case BadAskCheatPlayer:
                writer.WriteValue("a cheat");
                writer.WriteValue("bad-ask-for-tiles");
                break;
            case NoFitCheatPlayer:
                writer.WriteValue("a cheat");
                writer.WriteValue("no-fit");
                break;
            case BlockWin player:
                writer.WriteValue("win");
                writer.WriteValue(player.CountUntilBreak);
                break;
            case BlockSetup player:
                writer.WriteValue("setup");
                writer.WriteValue(player.CountUntilBreak);
                break;
            case BlockTakeTurn player:
                writer.WriteValue("take-turn");
                writer.WriteValue(player.CountUntilBreak);
                break;
            case BlockNewTiles player:
                writer.WriteValue("new-tiles");
                writer.WriteValue(player.CountUntilBreak);
                break;
        }
        writer.WriteEndArray();
    }

    public override bool CanWrite
    {
        get { return true; }
    }

    public override IPlayer ReadJson(
        JsonReader reader,
        Type objectType,
        IPlayer existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        if(array.Count > 4 || array.Count < 2)
        {
            throw new JsonSerializationException("Expected length 2, 3, or 4");
        }
        IStrategy strategy = array[1].ToObject<IStrategy>() ??
            throw new JsonSerializationException("Invalid Action");
        string name = array[0].ToObject<string>() ??
            throw new JsonSerializationException("Invalid player name");
        IPlayer player = new InHousePlayer(name, strategy);
        if (array.Count == 3)
        {
            player = array[2].ToObject<string>() switch
            {
                "setup" =>
                    new ExceptionalPlayer(player){BreakSetup = true},
                "take-turn" =>
                    new ExceptionalPlayer(player){BreakTakeTurn = true},
                "new-tiles"=>
                    new ExceptionalPlayer(player){BreakNewTiles = true},
                "win"=>
                    new ExceptionalPlayer(player){BreakWin = true},
                _ =>
                    throw new JsonSerializationException(
                        "Unknown JExn " + array[2].ToObject<string>())
            };
        }
        else if(array.Count == 4 && array[2].ToObject<string>() == "a cheat")
        {
            player = array[3].ToObject<string>() switch
            {
                "non-adjacent-coordinate" =>
                    new NonAdjCheatPlayer(name, strategy),
                "tile-not-owned" =>
                    new NotOwnedCheatPlayer(name, strategy),
                "not-a-line" =>
                    new NonLineCheatPlayer(name, strategy),
                "bad-ask-for-tiles" =>
                    new BadAskCheatPlayer(name, strategy),
                "no-fit" =>
                    new NoFitCheatPlayer(name, strategy),
                _ =>
                    throw new JsonSerializationException("Unknown jcheat " + array[3])
            };
        }
        else if(array.Count == 4 && array[2].ToObject<string>() != "a cheat")
        {
            player =  array[2].ToObject<string>() switch
            {
                "setup" =>
                    new BlockSetup(name, strategy, array[3].ToObject<int>()),
                "take-turn" =>
                    new BlockTakeTurn(name, strategy, array[3].ToObject<int>()),
                "new-tiles"=>
                    new BlockNewTiles(name, strategy, array[3].ToObject<int>()),
                "win"=>
                    new BlockWin(name, strategy, array[3].ToObject<int>()),
                _ =>
                    throw new JsonSerializationException("Unknown JExn " + array[2])
            };
        }
        return player;
    }
}
