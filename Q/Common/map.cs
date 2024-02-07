using System.Collections.Immutable;
using SkiaSharp;

namespace Q.Common;

/// The placement of tiles on the game map
public partial class Map : IRenderable
{
    private ImmutableDictionary<Coordinate, Tile> _tiles;

    /// Constructs a map from an initial tile at place (0, 0).
    public Map(Tile initialTile) : this(initialTile, new Coordinate(0, 0)) {}

    /// Constructs a map from an initial tile at the specified place.
    public Map(Tile initialTile, Coordinate coord)
    {
        _tiles = ImmutableDictionary.Create<Coordinate, Tile>()
            .Add(coord, initialTile);
    }

    private Map(ImmutableDictionary<Coordinate, Tile> tiles)
    {
        _tiles = tiles;
    }

    private Map() : this(ImmutableDictionary.Create<Coordinate, Tile>()) {}

    /// Retrieves the tile at the given coordinates, or null if there is no
    /// tile.
    public Tile? GetTile(Coordinate coordinate)
    {
        Tile tile;
        if (_tiles.TryGetValue(coordinate, out tile))
        {
            return tile;
        }
        return null;
    }

    /// Retrieves the up, down, left, and right neighbors of a tile. <summary>

    public Neighbors Neighbors(Coordinate coordinate)
    {
        var up = GetTile(new Coordinate(coordinate.X, coordinate.Y + 1));
        var down = GetTile(new Coordinate(coordinate.X, coordinate.Y - 1));
        var left = GetTile(new Coordinate(coordinate.X - 1, coordinate.Y));
        var right = GetTile(new Coordinate(coordinate.X + 1, coordinate.Y));
        return new Neighbors {
            Up = up,
            Down = down,
            Left = left,
            Right = right
        };
    }

    public IEnumerable<Placement> Placements()
    {
        return from kvp in _tiles
               orderby kvp.Key.Y, kvp.Key.X
               select new Placement(kvp.Key, kvp.Value);
    }

    /// Returns a map with the tile placed at the coordinate iff the tile to
    /// place has an adjacent tile on the map. Throws an
    /// InvalidOperationException otherwise. Does not validate the other game
    /// rules.
    public Map PlaceTile(Coordinate coordinate, Tile tile)
    {
        var neighbors = Neighbors(coordinate);
        var valid = neighbors.HasTile() && !_tiles.ContainsKey(coordinate);
        if (valid)
        {
            return SetTile(coordinate, tile);
        }
        else
        {
            throw new InvalidOperationException("No adjacent tile");
        }
    }

    /// Returns a map with placement applied if the tile to place would be
    /// connected to an existing tile. Throws an InvalidOperationException
    /// otherwise. Does not validate other game rules.
    public Map PlaceTile(Placement placement)
    {
        return PlaceTile(placement.Coordinate, placement.Tile);
    }

    public Map PlaceMultiple(IEnumerable<Placement> placements)
    {
        Map map = this;
        foreach (var placement in placements)
        {
            map = map.PlaceTile(placement.Coordinate, placement.Tile);
        }
        return map;
    }
    /// The set of valid placements of a tile according to the game rules.
    public IEnumerable<Coordinate> ValidPlacements(Tile tile)
    {
        return (
            from coord in _tiles.Keys
            from candidate in coord.Neighbors()
            where GetTile(candidate) == null
            && Neighbors(candidate).IsValidPlacement(tile)
            orderby candidate.Y, candidate.X
            select candidate
        ).Distinct();
    }

    //All potentiel places a tile could be placed
    public IEnumerable<Coordinate> OpenSpots()
    {
        return (
            from coord in _tiles.Keys
            from candidate in coord.Neighbors()
            where GetTile(candidate) == null
            orderby candidate.Y, candidate.X
            select candidate
        ).Distinct();
    }

    /// determines if the given placement is valid for this map
    public bool ValidPlacement(Placement placement)
    {
        return ValidPlacements(placement.Tile).Contains(placement.Coordinate);
    }

    /// Gets the entire row connected to the given placement
    public IEnumerable<Placement> GetConnectedRow(Placement placement)
    {
        if(GetTile(placement.Coordinate) != placement.Tile)
        {
            throw new ArgumentException("The starting tile is not on the map");
        }
        return new []{ placement }
            .Concat(GetConnectedOneDirection(
                        placement.Coordinate,
                        coord => coord with { X = coord.X - 1}))
            .Concat(GetConnectedOneDirection(
                        placement.Coordinate,
                        coord => coord with { X = coord.X + 1}));
    }

    public IEnumerable<Placement> GetConnectedCol(Placement placement)
    {
        if(GetTile(placement.Coordinate) != placement.Tile)
        {
            throw new ArgumentException("The starting tile is not on the map");
        }
        return new[]{ placement }
            .Concat(GetConnectedOneDirection(
                        placement.Coordinate,
                        coord => coord with { Y = coord.Y - 1}))
            .Concat(GetConnectedOneDirection(
                        placement.Coordinate,
                        coord => coord with { Y = coord.Y + 1}));
    }

    // Returns placements of all connected tiles in a given direction
    // `step`, not including starting placement at `coord`
    private IEnumerable<Placement> GetConnectedOneDirection(
        Coordinate coord,
        Func<Coordinate, Coordinate> step)
    {
        List<Placement> placements = new List<Placement>();
        Coordinate stepper = step(coord);
        var tile = GetTile(stepper);
        while (tile != null)
        {
            placements.Add(new Placement(stepper, tile));
            stepper = step(stepper);
            tile = GetTile(stepper);
        }
        return placements;
    }

    /// Returns a new map with the tile on the board at the coordinates.
    private Map SetTile(Coordinate coordinate, Tile tile)
    {
        return new Map(_tiles.SetItem(coordinate, tile));
    }

    public SKSurface Render(int width, int height)
    {
        SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        Bounds bounds = GetBounds();
        int tileSize = GetTileSize(bounds, width, height);
        int xOffset = DetermineXOffset(bounds, tileSize);
        int yOffset = DetermineYOffset(bounds, tileSize);
        foreach(Placement placement in Placements())
        {
            SKSurface tileSurface = placement.Tile.Render(tileSize, tileSize);
            canvas.DrawSurface(tileSurface,
                               xOffset + tileSize * placement.Coordinate.X,
                               yOffset + tileSize * placement.Coordinate.Y);
        }
        return surface;
    }

    private static int DetermineYOffset(Bounds bounds, int tileSize)
    {
        return -bounds.MinY * tileSize;
    }

    private static int DetermineXOffset(Bounds bounds, int tileSize)
    {
        return -bounds.MinX * tileSize;
    }

    private static int GetTileSize(Bounds bounds, int width, int height)
    {
        return Math.Min(width / (bounds.MaxX - bounds.MinX + 1),
                        height / (bounds.MaxY - bounds.MinY + 1));
    }

    /// <summary>
    /// Gets the bounds of a board
    /// </summary>
    /// <param name="placements">
    /// The list of placements to extract bounds from
    /// </param>
    /// <returns>The bounds of the board as  Bounds record</returns>
    public Bounds GetBounds()
    {
        return new Bounds(
                    MinX: Placements().Min(placement => placement.Coordinate.X),
                    MaxX: Placements().Max(placement => placement.Coordinate.X),
                    MinY: Placements().Min(placement => placement.Coordinate.Y),
                    MaxY: Placements().Max(placement => placement.Coordinate.Y));
    }

    /// <summary>
    /// Represents the boundaries of a board in terms of tile positions
    /// </summary>
    /// <param name="MinX">The minimum x tile position</param>
    /// <param name="MaxX">The maximum x tile position</param>
    /// <param name="MinY">The minimum y tile position</param>
    /// <param name="MaxY">The Maximum y tile position</param>
    public record Bounds(int MinX, int MaxX, int MinY, int MaxY);
}

/// A tile's neighbors. Empty spaces are represented by null.
public struct Neighbors
{
    public Tile? Up { get; set; }
    public Tile? Down { get; set; }
    public Tile? Left { get; set; }
    public Tile? Right { get; set; }

    /// Does a neighboring tile exist?
    public bool HasTile()
    {
        return Up != null || Down != null || Left != null || Right != null;
    }

    public int Count()
    {
        return (Up == null ? 0 : 1)
            + (Down == null ? 0 : 1)
            + (Left == null ? 0 : 1)
            + (Right == null ? 0 : 1);
    }

    /// Can the tile be placed inside the neighbors according to the game
    /// rules?
    public bool IsValidPlacement(Tile tile)
    {
        return HasTile()
            && IsValidLinePlacement(Up, tile, Down)
            && IsValidLinePlacement(Left, tile, Right);
    }

    private bool IsValidLinePlacement(Tile? prev, Tile toPlace, Tile? next)
    {
        return (toPlace.Color == (prev?.Color ?? toPlace.Color)
                && toPlace.Color == (next?.Color ?? toPlace.Color))
            || (toPlace.Shape == (prev?.Shape ?? toPlace.Shape)
                && toPlace.Shape == (next?.Shape ?? toPlace.Shape));
    }
}
