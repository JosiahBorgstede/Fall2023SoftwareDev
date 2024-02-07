using System.Net.Sockets;
using System.Net;
using Q.Server;
using Q.Player;
using Q.Referee;
using System.Diagnostics;
using Q.Common;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Q.Server;
public class server
{
    /// <summary>
    /// The max number of players allowed in this game
    /// </summary>
    private int _maxPlayers;

    /// <summary>
    /// The minimum number of players required to start a game
    /// </summary>
    private int _minPlayers;

    /// <summary>
    /// The amount of time per waiting period
    /// </summary>
    private TimeSpan _waitingPeriod;

    /// <summary>
    /// The amount of time to wait for a player to give their name
    /// </summary>
    private TimeSpan _nameTimeout;

    /// <summary>
    /// The number of waiting periods to run before stating the game or giving
    /// a default result
    /// </summary>
    private int _numWaitingPeriods;

    private RefereeConfiguration _refConfig;

    private int _port;

    private bool _debug;

    private TcpListener connector;

    public server(ServerConfiguration configuration)
    {
        _minPlayers = 2;
        _maxPlayers = 4;
        _waitingPeriod = configuration.TimePerWaitPeriod;
        _nameTimeout = configuration.PlayerNameWait;
        _port = configuration.PortNumber;
        _numWaitingPeriods = configuration.NumWaitingPeriods;
        _refConfig = configuration.refereeConfiguration;
        _debug = configuration.DebugEnabled;

    }

    public (IEnumerable<string>, IEnumerable<string>) StartSignup()
    {
        return StartSignup(_port);
    }

    /// <summary>
    /// Starts signup for a Q game from remote players
    /// </summary>
    /// <param name="portNum">The port to accept signups on</param>
    /// <returns>The arrays of winners and elliminated player names</returns>
    public (IEnumerable<string>, IEnumerable<string>) StartSignup(int portNum)
    {
        IEnumerable<IPlayer> playerList = new List<IPlayer>();
        connector = new TcpListener(IPAddress.Any, portNum);
        connector.Start();
        for(int i = 0; i < _numWaitingPeriods; i++)
        {
            if(_debug)
                Console.Error.WriteLine("Starting waiting period " + i);
            playerList = playerList.Concat(runWaitingPeriod(_waitingPeriod, _maxPlayers - playerList.Count()));
            if (playerList.Count() >= _minPlayers)
            {
                connector.Stop();
                return new RefereeDriver(_refConfig).Referee(playerList);
            }
        }
        connector.Stop();
        return (new List<string> { }, new List<string> { });
    }

    /// <summary>
    /// Runs a single waiting period for remote players to sign up
    /// </summary>
    /// <param name="waitPeriod">The amount of time this wait period should last</param>
    /// <param name="currentlyAcceptable">
    /// The max number of players acceptable buring this wait period
    /// </param>
    /// <returns>The list pf players accepted during this wait period</returns>
    public IEnumerable<IPlayer> runWaitingPeriod(TimeSpan waitPeriod, int currentlyAcceptable)
    {
        ConcurrentQueue<IPlayer> players = new();
        Stopwatch stopwatch = Stopwatch.StartNew();
        while(waitPeriod > stopwatch.Elapsed && players.Count < currentlyAcceptable)
        {
            if(connector.Pending())
            {
                connector.BeginAcceptTcpClient(ConnectOnePlayer, players);
            }
        }
        stopwatch.Stop();
        return players;
    }

    /// <summary>
    /// Connects a single  player to the game using a TCP connection, creating
    /// a ProxyPlayer.
    /// </summary>
    /// <param name="result">The asyncresult needed to make this operation asyncronous</param>
    private void ConnectOnePlayer(IAsyncResult result)
    {
        ConcurrentQueue<IPlayer> players = (ConcurrentQueue<IPlayer>) result.AsyncState!;
        TcpClient client = connector.EndAcceptTcpClient(result);
        NetworkStream stream = client.GetStream();
        JsonTextReader streamReader = new(new StreamReader(stream));
        try
        {
            var name = Task.Run(streamReader.ReadAsString);
            if(name.Wait(_nameTimeout)){
                IPlayer player = new ProxyPlayer(name.Result!, stream){DebugEnabled = _debug};
                players.Enqueue(player);
                if(_debug)
                    Console.Error.WriteLine("Added player: " + name.Result);
                return;
            }
            client.Close();
            return;
        }
        catch
        {
            client.Close();
            return;
        }
    }
}
