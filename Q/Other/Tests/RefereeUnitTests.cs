using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Moq;
using Q.Common;
using Q.Player;
using Q.Referee;
using Silk.NET.Core;

public class RefereeUnitTests
{
    struct Ref<T>
    {
        public T Inner;
    }

    private Mock<IPlayer> playerThatReturnsGoodActions(int scoreForActions, string name)
    {
        Mock<IAction> validAction = new();
        validAction
            .Setup(x => x.IsValid(It.IsAny<PlayerTurnInfo>()))
            .Returns(true);
        validAction
            .Setup(x => x.GetScore(It.IsAny<ScoreConfiguration>(),
                                   It.IsAny<PlayerTurnInfo>()))
            .Returns(scoreForActions);
        validAction
            .Setup(x => x.Commit(It.IsAny<IGameState>()))
            .Returns(new List<Tile>(){ new(Color.Orange, Shape.Star)});
        Mock<IPlayer> mockPlayer = new();
        mockPlayer
            .Setup(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()))
            .Returns(() => validAction.Object);
        mockPlayer.Setup(x => x.name()).Returns(name);
        return mockPlayer;
    }
    private Mock<IPlayer> playerThatReturnsBadActions(int scoreForActions, string name)
    {
        Mock<IAction> invalidAction = new();
        invalidAction
            .Setup(x => x.IsValid(It.IsAny<PlayerTurnInfo>()))
            .Returns(false);
        invalidAction
            .Setup(x => x.GetScore(It.IsAny<ScoreConfiguration>(),
                                   It.IsAny<PlayerTurnInfo>()))
            .Returns(scoreForActions);
        invalidAction
            .Setup(x => x.Commit(It.IsAny<IGameState>()))
            .Callback((IGameState state) => state.Map = new Map(new(Color.Red, Shape.Star)))
            .Returns(new List<Tile>(){ new(Color.Orange, Shape.Star)});
        Mock<IPlayer> mockPlayer = new();
        mockPlayer
            .Setup(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()))
            .Returns(() => invalidAction.Object);
        mockPlayer.Setup(x => x.name()).Returns(name);
        return mockPlayer;
    }
    private IGameState stateForNTurns(
        List<PlayerState> playersForTurns,
        List<PlayerState> players)
    {
        int n = 0;
        // Use indirection to make ReplaceCycle work. It needs to change
        // the cycle object to another instance.
        Ref<ICycle<PlayerState>> cycle = new Ref<ICycle<PlayerState>>
        {
            Inner = new Cycle<PlayerState>(new Queue<PlayerState>(players))
        };
        Map map = new Map(new(Color.Red, Shape.Star));
        IEnumerable<Tile> tiles = new Tile[]
        {
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
            new(Color.Red, Shape.Star),
        };
        var mockState = new GameState(map, cycle.Inner, tiles);

        return mockState;
    }

    [Fact]
    void TestReferee1()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>();
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Yellow, Shape.EightStar)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Yellow, Shape.EightStar)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);

        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        List<IPlayer> players = new(){player1, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Single(winners);
        Assert.Empty(eliminated);
        Assert.Contains("Alice", winners);
    }

    [Fact]
    void TestReferee2()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>();
        PlayerState  player1state = new(new Tile[]{
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);

        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        List<IPlayer> players = new(){player1, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Single(winners);
        Assert.Empty(eliminated);
        Assert.Contains("Bob", winners);
    }

    [Fact]
    void TestReferee3()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>();
        PlayerState  player1state = new(new Tile[]{
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);

        ExceptionalPlayer player1exn = new ExceptionalPlayer(new InHousePlayer("Alice", new Dag()))
        {
            BreakNewTiles = true
        };
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        List<IPlayer> players = new(){player1exn, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Single(winners);
        Assert.Single(eliminated);
        Assert.Contains("Bob", winners);
        Assert.Contains("Alice", eliminated);
    }

    [Fact]
    void TestReferee4()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>();
        PlayerState  player1state = new(new Tile[]{
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.Square)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);
        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        List<IPlayer> players = new(){player1, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Empty(eliminated);
        Assert.Contains("Bob", winners);
        Assert.Contains("Alice", winners);
    }

    [Fact]
    void TestReferee5()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>();
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.Diamond),
            new(Color.Yellow, Shape.Star),
            new(Color.Yellow, Shape.Circle),
            new(Color.Yellow, Shape.Clover),
            new(Color.Red, Shape.Clover),
            new(Color.Yellow, Shape.Square)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);

        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        List<IPlayer> players = new(){player1, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Empty(eliminated);
        Assert.Single(winners);
        Assert.Contains("Alice", winners);
        Assert.DoesNotContain("Bob", winners);
    }

    [Fact]
    void TestReferee6()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>()
        {
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
        };
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);

        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        List<IPlayer> players = new(){player1, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Empty(eliminated);
        Assert.Single(winners);
        Assert.Contains("Bob", winners);
        Assert.DoesNotContain("Alice", winners);
    }

    [Fact]
    void TestReferee7()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>()
        {
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
        };
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state});
        IGameState gameState = new GameState(map, cycle, tiles);

        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        ExceptionalPlayer player2 = new(new InHousePlayer("Bob", new Dag()))
        {
            BreakWin = true
        };
        List<IPlayer> players = new(){player1, player2};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.Single(eliminated);
        Assert.Empty(winners);
        Assert.Contains("Bob", eliminated);
        Assert.DoesNotContain("Alice", eliminated);
    }

    [Fact]
    void TestReferee8()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>()
        {
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
        };
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        PlayerState player3state = new(new Tile[]{
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover)}, "Joe");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state, player3state});
        IGameState gameState = new GameState(map, cycle, tiles);

        IPlayer player1 = new InHousePlayer("Alice", new Dag());
        ExceptionalPlayer player2 = new(new InHousePlayer("Bob", new Dag()))
        {
            BreakWin = true
        };
        ExceptionalPlayer player3 = new(new InHousePlayer("Joe", new Dag()))
        {
            BreakTakeTurn = true
        };
        List<IPlayer> players = new(){player1, player2, player3};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.NotEmpty(eliminated);
        Assert.Empty(winners);
        Assert.Contains("Bob", eliminated);
        Assert.Contains("Joe", eliminated);
        Assert.Equal(new string[]{"Joe", "Bob"}, eliminated);
        Assert.DoesNotContain("Alice", eliminated);
    }

    [Fact]
    void TestReferee9()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>()
        {
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
        };
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        PlayerState player3state = new(new Tile[]{
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover)}, "Joe");
        Cycle<PlayerState> cycle = new(new List<PlayerState>(){player1state, player2state, player3state});
        IGameState gameState = new GameState(map, cycle, tiles);

        ExceptionalPlayer player1 = new(new InHousePlayer("Alice", new Dag()))
        {
            BreakWin = true
        };
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        ExceptionalPlayer player3 = new(new InHousePlayer("Joe", new Dag()))
        {
            BreakTakeTurn = true
        };
        List<IPlayer> players = new(){player1, player2, player3};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.NotEmpty(eliminated);
        Assert.Single(winners);
        Assert.Contains("Bob", winners);
        Assert.Contains("Alice", eliminated);
        Assert.Contains("Joe", eliminated);
        Assert.Equal(new string[]{"Joe", "Alice"}, eliminated);
        Assert.DoesNotContain("Bob", eliminated);
    }

    [Fact]
    void TestReferee10()
    {
        Map map = new (new(Color.Yellow, Shape.EightStar));
        IEnumerable<Tile> tiles = new List<Tile>()
        {
            new(Color.Red, Shape.Diamond),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
            new(Color.Blue, Shape.EightStar),
        };
        PlayerState  player1state = new(new Tile[]{
            new(Color.Yellow, Shape.EightStar),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond),
            new(Color.Red, Shape.Diamond)}, "Alice");
        PlayerState player2state = new(new Tile[]{
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square),
            new(Color.Blue, Shape.Square)}, "Bob");
        PlayerState player3state = new(new Tile[]{
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Orange, Shape.Clover)}, "Joe");
        PlayerState player4state = new(new Tile[]{
            new(Color.Yellow, Shape.Clover),
            new(Color.Orange, Shape.Clover),
            new(Color.Green, Shape.Clover),
            new(Color.Purple, Shape.Circle),
            new(Color.Purple, Shape.Circle),
            new(Color.Purple, Shape.Circle)}, "John");
        Cycle<PlayerState> cycle = new(new List<PlayerState>()
        {
            player1state,
            player2state,
            player3state,
            player4state
        });
        IGameState gameState = new GameState(map, cycle, tiles);

        ExceptionalPlayer player1 = new(new InHousePlayer("Alice", new Dag()))
        {
            BreakWin = true
        };
        IPlayer player2 = new InHousePlayer("Bob", new Dag());
        ExceptionalPlayer player3 = new(new InHousePlayer("Joe", new Dag()))
        {
            BreakTakeTurn = true
        };
        IPlayer player4 = new InHousePlayer("John", new Dag());
        List<IPlayer> players = new(){player1, player2, player3, player4};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.NotEmpty(eliminated);
        Assert.Single(winners);
        Assert.Contains("Bob", winners);
        Assert.Equal(new string[]{"Joe", "Alice"}, eliminated);
        Assert.DoesNotContain("John", eliminated);
        Assert.DoesNotContain("John", winners);
    }

    [Fact]
    void TestReferee11()
    {
        IGameState gameState = new GameState.Builder()
                                            .RandomizeTiles()
                                            .AddPlayer("Alice")
                                            .AddPlayer("Bob")
                                            .AddPlayer("Joe")
                                            .AddPlayer("John")
                                            .Build();

        IPlayer player1 = new NonLineCheatPlayer("Alice", new Dag());
        Mock<IPlayer> player2 = new();
        player2.Setup(x => x.name()).Returns("Bob");
        player2.Setup(x => x.takeTurn(It.IsAny<PlayerTurnInfo>()))
               .Returns(() => {while(true);});
        IPlayer player3 = new NonAdjCheatPlayer("Joe", new Dag());
        IPlayer player4 = new InHousePlayer("John", new Dag());
        List<IPlayer> players = new(){player1, player2.Object, player3, player4};
        RefereeConfiguration config = new RefereeConfiguration.RefereeConfigBuilder()
                                          .TimePerTurnSeconds(3)
                                          .SetScoreConfiguration(new ScoreConfiguration(8, 4))
                                          .AddState(gameState)
                                          .Build();
        RefereeDriver refer = new RefereeDriver(config);
        (var winners, var eliminated) = refer.RefereeFromState(
            players,
            gameState);
        Assert.NotEmpty(eliminated);
        Assert.Contains("Alice", eliminated);
        Assert.Contains("Joe", eliminated);
        Assert.DoesNotContain("John", eliminated);
        Assert.Contains("Bob", eliminated);
    }
}
