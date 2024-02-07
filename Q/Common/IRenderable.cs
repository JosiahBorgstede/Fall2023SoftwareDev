using SkiaSharp;
using Silk.NET.Input;
namespace Q.Common;
/// <summary>
/// An object that can be rendered to a screen
/// </summary>
public interface IRenderable
{

    /// <summary>
    /// Returns the rendering of the object as a surface
    /// </summary>
    /// <param name="width">The width constraint the object should use</param>
    /// <param name="height">The height constraint the object should use</param>
    /// <returns>The surface with the object drawn on it</returns>
    public SKSurface Render(int width, int height);
}

public interface IInteractiveRenderable : IRenderable
{
    public void HandleKeyPress(IKeyboard keyboard, Key key, int value);
}