using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Q.Common;

namespace Q.Player;

public abstract record SingleAction
{
    public record Pass() : SingleAction();
    public record Replace() : SingleAction();
    public record OnePlacement(Placement Placement) : SingleAction();
    private SingleAction() {}
}

/// A strategy for playing the Q game.
[JsonConverter(typeof(IStrategyConverter))]
public interface IStrategy
{
    /// Apply the strategy once to get a placement. Returns null if no
    /// placement is possible.
    SingleAction ApplyStrategyOnce(PlayerTurnInfo info);

    /// Applies the strategy to get back a Q action.
    IAction ApplyStrategy(PlayerTurnInfo info);
}

/// An abstract strategy class that repeatedly applies ApplyStrategyOnce to
/// implement ApplyStrategy. If a placement isn't possible, attempts to
/// exchange tiles. If exchange is not possible, passes.
public abstract class GreedyStrategy : IStrategy
{

    public IAction ApplyStrategy(PlayerTurnInfo info)
    {
        return ApplyStrategyOnce(info) switch
        {
            SingleAction.Pass => new Pass(),
            SingleAction.Replace => new Exchange(),
            SingleAction.OnePlacement place => ApplyIteratively(info, place.Placement),
        };
    }

    public SingleAction ApplyStrategyOnce(PlayerTurnInfo info)
    {
        Placement? placement = FindFirstValidPlacement(info.Map,
                                                       info.CurrentPlayer.GetTiles());
        if(placement != null)
        {
            return new SingleAction.OnePlacement(placement);
        }
        if(new Exchange().IsValid(info))
        {
            return new SingleAction.Replace();
        }
        else
        {
            return new SingleAction.Pass();
        }
    }

    protected abstract Placement? FindFirstValidPlacement(
        Map map,
        IEnumerable<Tile> tiles);

    private Place ApplyIteratively(PlayerTurnInfo info, Placement? placement)
    {
        List<Placement> placements = new(){};
        List<Tile> newTiles = new List<Tile>(info.CurrentPlayer.GetTiles());
        do
        {
            placements.Add(placement);
            Map newMap = info.Map.PlaceMultiple(placements);
            newTiles.Remove(placement.Tile);
            placement = FindFirstValidPlacement(newMap, newTiles);
        }while(placement != null && new Place(placements.Append(placement)).IsValid(info));
        return new Place(placements);
    }
}

/// The dag strategy, which chooses the smallest tile (ordered
/// lexicographically by shape then color) that can be placed and breaks
/// coordinate ties by row-column order.
public class Dag : GreedyStrategy
{
    protected override Placement? FindFirstValidPlacement(
        Map map,
        IEnumerable<Tile> tiles)
    {
        tiles = from tile in tiles
                    orderby tile.Shape, tile.Color
                    select tile;
        foreach (var tile in tiles)
        {
            var coords = map.ValidPlacements(tile);
            if (coords.Any()) {
                return new(coords.First(), tile);
            }
        }
        return null;
    }
}

/// The ldasg strategy, which chooses the smallest tile (ordered
/// lexicographically by shape then color) that can be placed and breaks
/// coordinate ties by maximizing neighbors, then using row-column
/// order.
public class Ldasg : GreedyStrategy
{
    protected override Placement? FindFirstValidPlacement(
        Map map,
        IEnumerable<Tile> tiles)
    {
        tiles = from tile in tiles orderby tile.Shape, tile.Color select tile;
        foreach (var tile in tiles)
        {
            var coords = map.ValidPlacements(tile);
            if (coords.Any()) {
                var coordinate = (
                    from coord in coords
                    orderby map.Neighbors(coord).Count() descending,
                            coord.Y, coord.X
                    select coord
                ).First();
                return new Placement(coordinate, tile);
            }
        }
        return null;
    }
}


public class IStrategyConverter : JsonConverter<IStrategy>
{
    public override void WriteJson(
        JsonWriter writer,
        IStrategy value,
        JsonSerializer serializer)
    {
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override IStrategy ReadJson(
        JsonReader reader,
        Type objectType,
        IStrategy existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        switch ((string) token)
        {
            case "dag":
                return new Dag();
            case "ldasg":
                return new Ldasg();
            default:
                throw new JsonSerializationException("Unknown strategy");
        }
    }
}
