using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SkiaSharp;

namespace Q.Common;

/// The color of a tile piece.
[JsonConverter(typeof(StringEnumConverter))]
public enum Color
{
    [EnumMember(Value = "red")]
    Red,
    [EnumMember(Value = "green")]
    Green,
    [EnumMember(Value = "blue")]
    Blue,
    [EnumMember(Value = "yellow")]
    Yellow,
    [EnumMember(Value = "orange")]
    Orange,
    [EnumMember(Value = "purple")]
    Purple,
}

/// The shape of a tile piece.
[JsonConverter(typeof(StringEnumConverter))]
public enum Shape
{
    [EnumMember(Value = "star")]
    Star,
    [EnumMember(Value = "8star")]
    EightStar,
    [EnumMember(Value = "square")]
    Square,
    [EnumMember(Value = "circle")]
    Circle,
    [EnumMember(Value = "clover")]
    Clover,
    [EnumMember(Value = "diamond")]
    Diamond,
}

/// A coordinate pair representing a place on the game map.
public record Coordinate(
    [property: JsonProperty("column")] int X,
    [property: JsonProperty("row")] int Y)
{
    public IEnumerable<Coordinate> Neighbors()
    {
        return new Coordinate[] {
            new Coordinate(X - 1, Y),
            new Coordinate(X + 1, Y),
            new Coordinate(X, Y - 1),
            new Coordinate(X, Y + 1)
        };
    }
}

/// A tile piece that can be placed on the game board.
public record Tile([property: JsonProperty("color")] Color Color,
                   [property: JsonProperty("shape")] Shape Shape)
      : IRenderable
{

    public SKSurface Render(int width, int height)
    {
        SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        SKPaint paint = new(){Color = TileColor(Color)};
        int TileSize = Math.Min(width, height);
        switch (Shape)
        {
            case Shape.Star: DrawNStar(paint, canvas, 4, TileSize); break;
            case Shape.EightStar: DrawNStar(paint, canvas, 8, TileSize); break;
            case Shape.Square: DrawSquare(paint, canvas, TileSize); break;
            case Shape.Circle: DrawCircle(paint, canvas, TileSize); break;
            case Shape.Clover: DrawClover(paint, canvas, TileSize); break;
            case Shape.Diamond: DrawDiamond(paint, canvas, TileSize); break;
        };
        DrawBorder(canvas, width, height);
        return surface;
    }
    private void DrawBorder(SKCanvas canvas, int width, int height)
    {
        var paint = new SKPaint()
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawRect(0, 0, width, height, paint);
    }

    private void DrawCircle(SKPaint paint, SKCanvas canvas, int TileSize)
    {
        canvas.DrawCircle(TileSize / 2, TileSize / 2, TileSize / 2, paint);
    }

    private void DrawDiamond(SKPaint paint, SKCanvas canvas, int TileSize)
    {
        SKPath path = new() {FillType = SKPathFillType.EvenOdd};
        path.MoveTo(TileSize / 2 , 0);
        path.LineTo(TileSize, TileSize / 2);
        path.LineTo(TileSize / 2, TileSize);
        path.LineTo(0, TileSize / 2);
        path.LineTo(TileSize / 2, 0);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private void DrawClover(SKPaint paint, SKCanvas canvas, int TileSize)
    {
        int centerX = TileSize / 2;
        int centerY = TileSize / 2;
        canvas.DrawCircle(centerX + TileSize / 4, centerY, TileSize / 4, paint);
        canvas.DrawCircle(centerX - TileSize / 4, centerY, TileSize / 4, paint);
        canvas.DrawCircle(centerX, centerY + TileSize / 4, TileSize / 4, paint);
        canvas.DrawCircle(centerX, centerY - TileSize / 4, TileSize / 4, paint);
    }

    private void DrawSquare(SKPaint paint, SKCanvas canvas, int TileSize)
    {
        canvas.DrawRect(new SKRect(0, 0, TileSize, TileSize), paint);
    }

    private void DrawNStar(SKPaint paint, SKCanvas canvas, int n, int TileSize)
    {
        int centerX = TileSize / 2;
        int centerY = TileSize / 2;
        int smallR = TileSize / 4;
        int bigR = TileSize / 2;
        double theta = Math.PI / 2;
        SKPath path = new();
        path.MoveTo((float)(centerX + bigR * Math.Cos(theta)),
                    (float)(centerY - bigR * Math.Sin(theta)));
        for (int i = 0; i < n+1; ++i)
        {
            path.LineTo((float)(centerX + smallR * Math.Cos(theta + Math.PI / n)),
                        (float)(centerY - smallR * Math.Sin(theta + Math.PI / n)));
            theta = Math.PI / 2 + i * (2 * Math.PI / n);
            path.LineTo((float)(centerX + bigR * Math.Cos(theta)),
                        (float)(centerY - bigR * Math.Sin(theta)));
        }
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private SKColor TileColor(Color color) => color switch
    {
        Color.Red =>  SKColors.Red,
        Color.Yellow => SKColors.Yellow,
        Color.Purple => SKColors.Purple,
        Color.Orange => SKColors.Orange,
        Color.Green => SKColors.Green,
        Color.Blue => SKColors.Blue,
        _ => throw new ArgumentException("Invalid Color given")
    };
}
