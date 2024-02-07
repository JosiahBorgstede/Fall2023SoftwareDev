using System.Text;
using Newtonsoft.Json;
using Q.Common;
using Silk.NET.Input;
using SkiaSharp;

public interface IObserver
{
    /// <summary>
    /// Adds a state to the observer so it can display or use it.
    /// </summary>
    /// <param name="state">the state to add</param>
    public void AddState(IGameState state);

    /// <summary>
    /// Adds the final state of the game to the observer
    /// </summary>
    /// <param name="state">the state to add</param>
    public void AddWinningState(IGameState state);
}
public class GuiObserver : IObserver, IInteractiveRenderable
{
    public List<IGameState> States;

    private int current;

    private int _width;
    private int _height;

    /// <summary>
    /// Creates an Observer
    /// </summary>
    /// <param name="width">The width to use when rendering game states</param>
    /// <param name="height">The height to use when rendering game states</param>
    public GuiObserver(int width = 600, int height = 600)
    {
        States = new();
        _width = width;
        _height = height;
        if(Directory.Exists("Tmp"))
        {
            Directory.Delete("Tmp", true);
        }
        Directory.CreateDirectory("Tmp");
    }
    public void AddState(IGameState state)
    {
        States.Add(new GameState(state));
        SaveAsPng(States.Count()-1);
    }

    public void AddWinningState(IGameState state)
    {
        AddState(state);
    }

    public void HandleKeyPress(IKeyboard keyboard, Key key, int value)
    {
        if(key == Key.Left)
        {
            current = current == 0 ? 0 : current - 1;
        }

        if(key == Key.Right)
        {
            current = current == States.Count - 1 ? States.Count - 1 : current + 1;
        }

        if(key == Key.S)
        {
            SaveAsJState("Tmp/state-" + current + ".json", current);
        }
    }

    public SKSurface Render(int width, int height)
    {
        if(States.Any())
        {
            return States[current].Render(width, height);
        }
        else {
            return SKSurface.Create(new SKImageInfo(_width, _height));
        }
    }

    public void SaveAsJState(string filepath, int toSave)
    {
        string jstate = JsonConvert.SerializeObject(States[toSave]);
        FileStream file = File.OpenWrite(filepath);
        file.Write(Encoding.UTF8.GetBytes(jstate));
    }

    public void SaveAsPng(int toSave)
    {
        SKSurface surface = SKSurface.Create(new SKImageInfo(_width, _height));
        surface.Canvas.Clear(SKColors.White);
        surface.Canvas.DrawSurface(States[toSave].Render(_width, _height), 0, 0);
        SKImage image = surface.Snapshot();
        SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        FileStream file = File.OpenWrite("Tmp/" + toSave + ".png");
        data.SaveTo(file);
    }
}