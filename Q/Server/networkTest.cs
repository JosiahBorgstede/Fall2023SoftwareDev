using Newtonsoft.Json;
using Q.Client;
using Q.Common;
using Q.Player;
using Q.Server;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: ./xtcp <port>");
            return 1;
        }
        ServerConfiguration config = new ServerConfiguration.ServerConfigBuilder()
                                         .SetPortNumber(12345)
                                         .SetPlayerNameWaitTime(6)
                                         .SetServerTries(2)
                                         .SetServerWaitTime(20)
                                         .Build();
        var server = new server(config);
        IPlayer player1 = new InHousePlayer("foo", new Dag());
        Client client1 = new(player1, false);
        IPlayer player2 = new InHousePlayer("bar", new Dag());
        Client client2 = new(player2, false);
        IPlayer player3 = new InHousePlayer("baz", new Dag());
        Client client3 = new(player3, false);
        Thread cl1 = new Thread(() => {Thread.Sleep(2000); client1.JoinAndRunGame(12345, "localhost");});
        Thread cl2 = new Thread(() => {Thread.Sleep(3000); client2.JoinAndRunGame(12345, "localhost");});
        Thread cl3 = new Thread(() => {Thread.Sleep(4000); client3.JoinAndRunGame(12345, "localhost");});
        cl1.Start();
        cl2.Start();
        cl3.Start();

        (var winners, var kicked) = server.StartSignup(12345);
        Console.WriteLine("[" +
            JsonConvert.SerializeObject(winners) + "," +
            JsonConvert.SerializeObject(kicked) + "]");
        Console.Error.WriteLine(cl1.ThreadState);
        Console.Error.WriteLine(cl2.ThreadState);
        Console.Error.WriteLine(cl3.ThreadState);
        return 0;
    }
}