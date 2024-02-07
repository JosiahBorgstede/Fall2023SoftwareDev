using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace Q.Common;

/// The data of a player to be used by the game state <summary>
/// The data of a player to be used by the game state
/// </summary>
public class PlayerState : IRenderable
{
    [property: JsonProperty("tile*")]
    public List<Tile> Tiles {get;}

    [property: JsonProperty("score")]
    public int Score { get; set; }

    [property: JsonProperty("name")]
    public string Name { get; set; }

    /// Constructs a Player from an inventory of tiles.
    public PlayerState(IEnumerable<Tile> tiles, string name)
    {
        Score = 0;
        Tiles = new List<Tile>(tiles);
        Name = name;
    }

    /// Constructs a Player from just a name.
    public PlayerState(string name)
    {
        Score = 0;
        Tiles = new List<Tile>();
        Name = name;
    }

    public PlayerState(PlayerState state)
    {
        Score = state.Score;
        Tiles = new List<Tile>(state.Tiles);
        Name = state.Name;
    }

    [JsonConstructor]
    public PlayerState([JsonProperty("tile*")] IEnumerable<Tile> tiles,
                       [JsonProperty("score")] int score,
                       [JsonProperty("name")] string name)
    {
        Score = score;
        Tiles = new List<Tile>(tiles);
        Name = name;
    }

    public IEnumerable<Tile> GetTiles()
    {
        return Tiles.AsEnumerable();
    }

    /// Gives the player the tile.
    public void AddTile(Tile tile)
    {
        Tiles.Add(tile);
    }

    public void AddTiles(IEnumerable<Tile> toAdd)
    {
        foreach(var tile in toAdd)
        {
            Tiles.Add(tile);
        }
    }

    public void RemoveTiles(IEnumerable<Tile> toRemove)
    {
        foreach(var tile in toRemove)
        {
            RemoveTile(tile);
        }
    }

    /// Removes the tile from the player's possession. Throws an
    /// IllegalArgumentException if the player does not have the tile.
    private void RemoveTile(Tile tile)
    {
        if (!Tiles.Remove(tile))
        {
            throw new ArgumentException("Player does not have tile");
        }
    }

    /// The player's number of tiles.
    public int TileCount()
    {
        return Tiles.Count;
    }

    /// Projects the information about a player that is visible to all players
    /// and spectators
    public PublicPlayerInfo PublicInfo()
    {
        return new PublicPlayerInfo(Score);
    }

    public SKSurface Render(int width, int height)
    {
        SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        DrawScoreAndName(canvas, width, height/5);
        DrawHand(canvas, width, height, height/5);
        return surface;
    }
    /// <summary>
    /// Draws the players hand
    /// </summary>
    /// <param name="canvas">The canvas to draw each part of the hand on</param>
    /// <param name="width">The width limit for drawing the hand</param>
    /// <param name="height">The height limit for drawing the hand</param>
    /// <param name="yOffset">The y offset to start at in the canvas</param>
    private void DrawHand(SKCanvas canvas, int width, int height, int yOffset)
    {
        int tileWidth = height;
        if(Tiles.Count != 0)
        {
            tileWidth = Math.Min(height, width/Tiles.Count);
        }
        for(int i = 0; i < Tiles.Count(); ++i)
        {
            canvas.DrawSurface(Tiles[i].Render(tileWidth, tileWidth),
                               i * tileWidth,
                               yOffset);
        }
    }

    private void DrawScoreAndName(SKCanvas canvas, int width, int height)
    {
        using SKPaint scorePaint = new() {Color = SKColors.Black};
        scorePaint.TextSize = height * 0.8f;
        scorePaint.TextAlign = SKTextAlign.Center;
        canvas.DrawText(Name + " Score: " + Score,
                        width / 2,
                        height * 0.8f,
                        scorePaint);
    }
}


