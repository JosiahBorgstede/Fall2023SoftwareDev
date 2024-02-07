using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Q.Common;
using Q.Player;
namespace Q.Client;

public class ProxyReferee
{
    private IPlayer _player;

    private JsonTextReader _readStream;
    private JsonTextWriter _writeStream;

    public bool running;

    public bool _debug;

    public ProxyReferee(Stream stream, IPlayer player, bool DebugEnabled)
           : this(new StreamReader(stream), new StreamWriter(stream), player, DebugEnabled)
    {}

    public ProxyReferee(StreamReader toRead, StreamWriter toWrite, IPlayer player, bool DebugEnabled)
    {

        _readStream = new JsonTextReader(new CharacterReader(toRead)){SupportMultipleContent = true};
        _writeStream = new JsonTextWriter(toWrite);
        _player = player;
        _debug = DebugEnabled;
        running = false;
    }


    /// <summary>
    /// Writes the players name onto the stream, then starts this player
    /// listening on a different thread
    /// </summary>
    public void Start()
    {
        _writeStream.WriteValue(_player.name());
        _writeStream.Flush();
        running = true;
        Thread listening = new Thread(new ThreadStart(Listen));
        listening.Start();
    }


    /// <summary>
    /// Stops the listening loop
    /// </summary>
    public void Stop()
    {
        running = false;
    }

    /// <summary>
    /// Begins to listen on the stream, running until stopped or the game ends
    /// </summary>
    public void Listen()
    {
        while(running)
        {
            try
            {
                if(!Task.Run(ReadInValue).Wait(TimeSpan.FromSeconds(200)))
                {
                    if(_debug)
                        Console.Error.WriteLine("things took too long");
                    return;
                }
            }
            catch (Exception e)
            {
                if(_debug)
                    Console.Error.WriteLine("Reading values from network broke: " + e.Message);
                return;
            }
        }
        if(_debug)
            Console.Error.WriteLine("Client finished");
    }

    /// <summary>
    /// Reads in a value from the stream and calls the appropriate player method,
    /// writing the return value of that method back onto the stream
    /// </summary>
    public void ReadInValue()
    {
        JsonSerializer serializer = JsonSerializer.Create();
        if(_debug)
            Console.Error.WriteLine("starting read for player " + _player.name());
        _readStream.Read();
        if(_debug)
            Console.Error.WriteLine("read something for player " + _player.name());
        var result = CallPlayerMethod(JArray.Load(_readStream));
        if(_debug)
            Console.Error.WriteLine(result + " from player: " + _player.name());
        serializer.Serialize(_writeStream, result);
        _writeStream.Flush();
        
    }

    /// <summary>
    /// Converts an array of JSON objects into a call on the player of this object
    /// </summary>
    /// <param name="array">The array to be converted to a player call</param>
    /// <returns>
    /// The result the player returned. If the method is void, will return the
    /// string "void"
    /// </returns>
    /// <exception cref="JsonReaderException">
    /// Thrown if unable to convert the array to a method call
    /// </exception>
    private object? CallPlayerMethod(JArray array)
    {
        string mname = array[0].ToObject<string>()
                       ?? throw new JsonReaderException("Method name read as null");
        if(_debug)
        {
            Console.Error.WriteLine("Calling method: "+ mname
                                    + " on player: " + _player.name());
        }
        switch (mname)
        {
            case "setup":
                _player.setup(array[1].ToObject<PlayerTurnInfo>(),
                              array[2].ToObject<IEnumerable<Tile>>());
                return "void";
            case "new-tiles":
                _player.newTiles(array[1].ToObject<IEnumerable<Tile>>());
                return "void";
            case "take-turn":
                return _player.takeTurn(array[1].ToObject<PlayerTurnInfo>());
            case "win":
                _player.win(array[1].ToObject<bool>());
                running = false;
                return "void";
            default:
                throw new JsonReaderException("Not a valid method");
        }
    }
}
