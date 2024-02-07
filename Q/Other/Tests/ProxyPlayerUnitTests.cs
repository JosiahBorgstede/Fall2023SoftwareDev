using Q.Server;
using Q.Client;
using Moq;
using Q.Player;
using Q.Common;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;

public class ProxyPlayerUnitTests
{
    //Extracts the entire memory stream as a string, removing trailing null chars
    public string CleanMemoryStream(MemoryStream stream)
    {
        return Encoding.UTF8.GetString(stream.GetBuffer()).Trim('\0');
    }

    [Fact]
    public void PlayerTest1()
    {
        MemoryStream memoryStream = new();
        MemoryStream readStream = SetupStream("\"void\"");
        ProxyPlayer player = new("foo", new StreamReader(readStream), new StreamWriter(memoryStream));
        player.win(true);
        object[] mcall = {"win", true};
        Assert.Equal(JsonConvert.SerializeObject(mcall),CleanMemoryStream(memoryStream));
    }

    [Fact]
    public void PlayerTest2()
    {
        MemoryStream memoryStream = new();
        MemoryStream readStream = SetupStream("\"void\"");
        ProxyPlayer player = new("foo", new StreamReader(readStream), new StreamWriter(memoryStream));
        List<Tile> toGive = new(){new(Color.Red, Shape.Star), new(Color.Green, Shape.Square)};
        player.newTiles(toGive);
        object[] mcall = {"new-tiles", toGive};
        Assert.Equal(JsonConvert.SerializeObject(mcall),CleanMemoryStream(memoryStream));
    }

    [Fact]
    public void PlayerTest3()
    {
        MemoryStream memoryStream = new();
        MemoryStream readStream = SetupStream("\"void\"");
        ProxyPlayer player = new("foo", new StreamReader(readStream), new StreamWriter(memoryStream));
        GameState state = new GameState.Builder()
                              .RandomizeTiles()
                              .AddPlayer("foo")
                              .AddPlayer("bar")
                              .Build();
        player.setup(state.PublicInfo(), state.PublicInfo().CurrentPlayer.GetTiles());
        object[] mcall = {"setup", state.PublicInfo(), state.PublicInfo().CurrentPlayer.GetTiles()};
        Assert.Equal(JsonConvert.SerializeObject(mcall),CleanMemoryStream(memoryStream));
    }

    [Fact]
    public void PlayerTest4()
    {
        MemoryStream memoryStream = new();
        MemoryStream readStream = SetupStream(JsonConvert.SerializeObject(new Pass()));
        ProxyPlayer player = new("foo", new StreamReader(readStream), new StreamWriter(memoryStream));
        GameState state = new GameState.Builder()
                              .RandomizeTiles()
                              .AddPlayer("foo")
                              .AddPlayer("bar")
                              .Build();
        player.takeTurn(state.PublicInfo());
        object[] mcall = {"take-turn", state.PublicInfo()};
        Assert.Equal(JsonConvert.SerializeObject(mcall),CleanMemoryStream(memoryStream));
    }

    [Fact]
    public void PlayerTest6()
    {
        MemoryStream memoryStream = new();
        ProxyPlayer player = new("foo", memoryStream);
        Assert.Equal("foo", player.name());
    }

    //Creates a stream with input written on it, with the stream position set
    //at the beginning.
    private MemoryStream SetupStream(string input)
    {
        MemoryStream memoryStream = new(10000);
        StreamWriter writer = new(memoryStream);
        writer.Write(input);
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    [Fact]
    public void RefereeTest1()
    {
        object[] mcall = {"win", true};
        var input = JsonConvert.SerializeObject(mcall);
        var inputStream = SetupStream(input);
        Mock<IPlayer> mockPlayer = new();
        ProxyReferee referee = new(inputStream, mockPlayer.Object, false);
        referee.ReadInValue();
        mockPlayer.Verify(x => x.win(true));
        Assert.Equal(input + "\"void\"",CleanMemoryStream(inputStream));
    }

    [Fact]
    public void RefereeTest2()
    {
        List<Tile> toGive = new(){new(Color.Red, Shape.Star), new(Color.Green, Shape.Square)};
        object[] mcall = {"new-tiles", toGive};
        var input = JsonConvert.SerializeObject(mcall);
        var inputStream = SetupStream(input);
        Mock<IPlayer> mockPlayer = new();
        ProxyReferee referee = new(inputStream, mockPlayer.Object, false);
        referee.ReadInValue();
        mockPlayer.Verify(x => x.newTiles(toGive));
        Assert.Equal(input + "\"void\"",CleanMemoryStream(inputStream));
    }

    [Fact]
    public void RefereeTest3()
    {
        GameState state = new GameState.Builder()
                              .RandomizeTiles()
                              .AddPlayer("foo")
                              .AddPlayer("bar")
                              .Build();
        object[] mcall = {"take-turn", state.PublicInfo()};
        var input = JsonConvert.SerializeObject(mcall);
        var inputStream = SetupStream(input);
        Mock<IPlayer> mockPlayer = new();
        mockPlayer.Setup(x => x.takeTurn(It.IsAny<PlayerTurnInfo>())).Returns(new Pass());
        ProxyReferee referee = new(inputStream, mockPlayer.Object, false);
        referee.ReadInValue();
        mockPlayer.Verify(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()));
        Assert.Equal(input + "\"pass\"",CleanMemoryStream(inputStream));
    }

    [Fact]
    public void RefereeTest4()
    {
        GameState state = new GameState.Builder()
                              .RandomizeTiles()
                              .AddPlayer("foo")
                              .AddPlayer("bar")
                              .Build();
        object[] mcall = {"take-turn", state.PublicInfo()};
        var input = JsonConvert.SerializeObject(mcall);
        var inputStream = SetupStream(input);
        Mock<IPlayer> mockPlayer = new();
        mockPlayer.Setup(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()))
                  .Returns(new Exchange());
        ProxyReferee referee = new(inputStream, mockPlayer.Object, false);
        referee.ReadInValue();
        mockPlayer.Verify(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()));
        Assert.Equal(input + "\"exchange\"",CleanMemoryStream(inputStream));
    }

    [Fact]
    public void RefereeTest5()
    {
        GameState state = new GameState.Builder()
                              .RandomizeTiles()
                              .AddPlayer("foo")
                              .AddPlayer("bar")
                              .Build();
        object[] mcall = {"take-turn", state.PublicInfo()};
        var input = JsonConvert.SerializeObject(mcall);
        var inputStream = SetupStream(input);
        Mock<IPlayer> mockPlayer = new();
        List<Placement> placeMove = new()
        {
            new(new(0,0), new(Color.Green, Shape.Star)),
            new(new(1,0), new(Color.Green, Shape.Square)),
            new(new(2,0), new(Color.Green, Shape.Diamond))
        };
        mockPlayer.Setup(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()))
                  .Returns(new Place(placeMove));
        ProxyReferee referee = new(inputStream, mockPlayer.Object, false);
        referee.ReadInValue();
        mockPlayer.Verify(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()));
        var Jmove = JsonConvert.SerializeObject(placeMove);
        Assert.Equal(input + Jmove,CleanMemoryStream(inputStream));
    }

    [Fact]
    public void RefereeTest6()
    {
        GameState state = new GameState.Builder()
                              .RandomizeTiles()
                              .AddPlayer("foo")
                              .AddPlayer("bar")
                              .Build();
        object[] mcall = {"setup", state.PublicInfo(), state.PublicInfo().CurrentPlayer.GetTiles()};
        var input = JsonConvert.SerializeObject(mcall);
        var inputStream = SetupStream(input);
        Mock<IPlayer> mockPlayer = new();
        ProxyReferee referee = new(inputStream, mockPlayer.Object, false);
        referee.ReadInValue();
        mockPlayer.Verify(x => x.setup(It.IsAny<PlayerTurnInfo>(), It.IsAny<IEnumerable<Tile>>()));
        Assert.Equal(input + "\"void\"",CleanMemoryStream(inputStream));
    }
}