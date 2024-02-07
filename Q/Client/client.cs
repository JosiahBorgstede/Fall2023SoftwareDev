using System.Diagnostics.Metrics;
using System.Net.Sockets;
using Q.Player;

namespace Q.Client;

public class Client
{
    IPlayer Player;

    bool _debug;

    public Client(IPlayer player, bool DebugEnabled)
    {
        _debug = DebugEnabled;
        Player = player;
    }

    public void JoinAndRunGame(int port, string hostname)
    {
        var client = AttemptConnection(hostname, port);
        ProxyReferee referee = new(client.GetStream(), Player, _debug);
        referee.Start();
    }

    private TcpClient AttemptConnection(string hostname, int port)
    {
        for(int i = 0; i < 10; i++)
        {
            try
            {
                TcpClient client = new();
                return new TcpClient(hostname, port);
            }
            catch
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                continue;
            }
        }
        if(_debug)
            Console.Error.WriteLine("Unable to connect to game");
        throw new Exception("Unable to connect");
    }
}

