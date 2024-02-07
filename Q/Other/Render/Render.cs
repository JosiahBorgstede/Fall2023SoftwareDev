using Q.Common;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using Silk.NET.Input;
using SkiaSharp;

public class Renderer
{
    WindowOptions options;
    IWindow window;
    GRContext grContext;

    public Renderer(int width, int height)
    {
        options = WindowOptions.Default;
        options.Title = "Q";
        options.Size = new Vector2D<int>(width, height);
        GlfwWindowing.Use();
        grContext = GRContext.CreateGl();
        window = Window.Create(options);
    }
    /// <summary>
    /// Renders the given renderable to a window
    /// </summary>
    /// <param name="toRender">The object to render</param>
    public void RenderToWindow(IRenderable toRender)
    {
        window.Initialize();
        grContext = GRContext.CreateGl();
        RunWindow(toRender);
    }

    /// <summary>
    /// Runs a window, displaying what needs to be displayed
    /// </summary>
    /// <param name="toRender"></param>
    private void RunWindow(IRenderable toRender)
    {
        window.Render += d =>
        {
            using var renderTarget = new GRBackendRenderTarget(window.Size.X,
                                                               window.Size.Y,
                                                               0,
                                                               0,
                                                               new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8
            using var surface = SKSurface.Create(grContext,
                                                 renderTarget,
                                                 GRSurfaceOrigin.BottomLeft,
                                                 SKColorType.Rgba8888);
            using var canvas = surface.Canvas;
            grContext.ResetContext();
            canvas.Clear(SKColors.Cyan);
            canvas.DrawSurface(toRender.Render(window.Size.X, window.Size.Y),
                               0,
                               0);
            canvas.Flush();
        };

        window.Run();
    }

    public void RenderToInteractiveWindow(IInteractiveRenderable toRender)
    {
        window.Initialize();
        grContext = GRContext.CreateGl();
        IInputContext input = window.CreateInput();
        input.Keyboards.First().KeyDown += toRender.HandleKeyPress;
        RunWindow(toRender);
    }
}
