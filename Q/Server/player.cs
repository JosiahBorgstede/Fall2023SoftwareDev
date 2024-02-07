using Newtonsoft.Json;
using Q.Common;
using Q.Player;
namespace Q.Server;

public class ProxyPlayer : IPlayer
{
    public IStrategy _strat { get; }

    public bool GameOver => false;

    public bool Won => false;

    private string _name;

    private JsonReader _readStream;

    private JsonWriter _writeStream;

    public bool DebugEnabled {get; set;}

    public ProxyPlayer(string name, TextReader toRead, TextWriter toWrite)
    {
        _name = name;
        _readStream = new JsonTextReader(new CharacterReader(toRead)) {SupportMultipleContent = true};
        _writeStream = new JsonTextWriter(toWrite);
    }

    public ProxyPlayer(string name, Stream stream)
        : this(name, new StreamReader(stream), new StreamWriter(stream))
    {}

    public string name()
    {
        return _name;
    }

    public void newTiles(IEnumerable<Tile> st)
    {
        IEnumerable<object> toSend = new List<object>(){"new-tiles", st};
        if(DebugEnabled)
            Console.Error.WriteLine("Calling Method: newTiles on " + _name);
        SendOnStream(toSend);
        string reply = RecieveReply<string>();
        if(DebugEnabled)
            Console.Error.WriteLine("newTiles reply " + reply + " from player " + _name);
        if (reply != "void")
        {
            throw new JsonReaderException("newTiles did not recieve void back");
        }
    }

    public void setup(PlayerTurnInfo info, IEnumerable<Tile> st)
    {
        IEnumerable<object> toSend = new List<object>(){"setup",info, st};
        if(DebugEnabled)
            Console.Error.WriteLine("Calling Method: setup on " + _name);
        SendOnStream(toSend);
        string reply = RecieveReply<string>();
        if(DebugEnabled)
            Console.Error.WriteLine("setup reply " + reply + " from " + _name);
        if (reply != "void")
        {
            throw new JsonReaderException("setup did not recieve void back");
        }
    }

    public IAction takeTurn(PlayerTurnInfo p)
    {
        IEnumerable<object> toSend = new List<object>(){"take-turn",p};
        if(DebugEnabled)
            Console.Error.WriteLine("Calling Method: takeTurn on " + _name);
        SendOnStream(toSend);
        IAction reply = RecieveReply<IAction>();
        if(DebugEnabled)
            Console.Error.WriteLine("take turn reply " + reply + " from " + _name);
        return reply;
    }

    /// <summary>
    /// Calls win on the player
    /// </summary>
    /// <param name="w">true if this player won, false otherwise</param>
    /// <exception cref="JsonReaderException">If "void" was not returned</exception>
    public void win(bool w)
    {
        List<object> toSend = new List<object>(){"win",w};
        if(DebugEnabled)
            Console.Error.WriteLine("Calling Method: win on " + _name);
        SendOnStream(toSend);
        string reply = RecieveReply<string>();
        if(DebugEnabled)
            Console.Error.WriteLine("win reply " + reply + " from " + _name);
        if (reply != "void")
        {
            throw new JsonReaderException("win did not recieve void back");
        }
    }

    /// <summary>
    /// Writes the given array of objects onto the stream of this player,
    /// serializing the objects to JSON.
    /// </summary>
    /// <param name="toSend">The objects to be serialized and written</param>
    private void SendOnStream(IEnumerable<object> toSend)
    {
        JsonSerializer serializer = new();
        serializer.Serialize(_writeStream, toSend);
        _writeStream.Flush();
    }

    /// <summary>
    /// Deserializes a T from JSON from the stream of this player
    /// </summary>
    /// <typeparam name="T">The type to be deserialized</typeparam>
    /// <returns>An object of type T</returns>
    /// <exception cref="JsonReaderException">
    /// Thrown if fails to deserialze, or if deserialized as null
    /// </exception>
    private T RecieveReply<T>()
    {
        JsonSerializer serializer = new();
        _readStream.Read();
        return serializer.Deserialize<T>(_readStream) ??
            throw new JsonReaderException("Unable to read replay");
    }
}
