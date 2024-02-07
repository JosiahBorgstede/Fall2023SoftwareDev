using Q.Common;

namespace Other;

// To render a window containtaing a game state simply add these:
// Renderer rendered = new(width, height);
// rendered.RenderToWindow(state);
// With width and height being the width and height of the window to open
// and state being the state to be rendered
public class GameStateUnitTests
{

    [Fact]
    public void TestAddPlayer()
    {
        var state = new GameState.Builder()
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();

        PlayerTurnInfo gameInfo = state.PublicInfo();
        Assert.Single(gameInfo.PlayerInfos);
    }
    [Fact]
    public void TestNoPlayers()
    {
        var builder = new GameState.Builder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void TestOnePlayer()
    {
        var builder = new GameState.Builder()
            .AddPlayer("foo");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void TestTwoPlayers()
    {
        var state = new GameState.Builder()
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
    }

    [Fact]
    public void TestThreePlayers()
    {
        var state = new GameState.Builder()
            .AddPlayer("foo")
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
    }

    [Fact]
    public void TestFourPlayers()
    {
        var state = new GameState.Builder()
            .AddPlayer("foo")
            .AddPlayer("foo")
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
    }

    [Fact]
    public void TestFivePlayers()
    {
        var builder = new GameState.Builder()
            .AddPlayer("foo")
            .AddPlayer("foo")
            .AddPlayer("foo")
            .AddPlayer("foo")
            .AddPlayer("foo");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    public Tile[] TileFixture()
    {
        return new [] {
            new Tile(Color.Blue, Shape.Clover),
            new Tile(Color.Green, Shape.Star),
            new Tile(Color.Blue, Shape.Circle),
            new Tile(Color.Yellow, Shape.Square),
            new Tile(Color.Yellow, Shape.Diamond),
            new Tile(Color.Blue, Shape.Star),

            new Tile(Color.Green, Shape.EightStar),
            new Tile(Color.Green, Shape.Clover),
            new Tile(Color.Red, Shape.Diamond),
            new Tile(Color.Orange, Shape.Diamond),
            new Tile(Color.Purple, Shape.Clover),
            new Tile(Color.Blue, Shape.Clover),

            new Tile(Color.Green, Shape.Clover),

            new Tile(Color.Orange, Shape.Clover),
            new Tile(Color.Purple, Shape.Diamond),
        };
    }

    [Fact]
    public void TestFirstMovePlace1()
    {
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
        var action = new Place(new [] {
                new Placement(new(1, 0), new Tile(Color.Blue, Shape.Clover)),
                new Placement(new(2, 0), new Tile(Color.Blue, Shape.Star)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        action.Commit(state);
    }

    [Fact]
    public void TestFirstMovePlace2()
    {
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
        var action = new Place(new [] {
                new Placement(new(1, 0), new Tile(Color.Blue, Shape.Clover)),
                new Placement(new(0, 1), new Tile(Color.Green, Shape.Star)),
            });
        Assert.False(action.IsValid(state.PublicInfo()));
    }

    [Fact]
    public void TestFirstMovePlace3()
    {
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
        var action = new Place(new [] {
                new Placement(new(1, 0), new Tile(Color.Blue, Shape.Clover)),
                new Placement(new(2, 0), new Tile(Color.Orange, Shape.Clover)),
            });
        Assert.False(action.IsValid(state.PublicInfo()));
    }

    [Fact]
    public void TestFirstMovePlace4()
    {
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
        var action = new Place(new [] {
                new Placement(new(1, 0), new Tile(Color.Blue, Shape.Clover)),
                new Placement(new(10, 10), new Tile(Color.Green, Shape.Star)),
            });
        Assert.False(action.IsValid(state.PublicInfo()));
    }

    [Fact]
    public void TestFirstMoveExchange1()
    {
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
        var action = new Exchange();
        Assert.False(action.IsValid(state.PublicInfo()));
    }

    // Score counts both player sequences and extended sequences
    [Fact]
    public void TestPlaceScore1()
    {
        //
        // RD  R8 RSt _
        // GD         _
        // YD YCr YCv _
        //
        Map testMap = new Map(new Tile(Color.Red, Shape.Star))
                    .PlaceTile(new(-1, 0), new(Color.Red, Shape.EightStar))
                    .PlaceTile(new(-2, 0), new(Color.Red, Shape.Diamond))
                    .PlaceTile(new(-2, 1), new(Color.Green, Shape.Diamond))
                    .PlaceTile(new(-2, 2), new(Color.Yellow, Shape.Diamond))
                    .PlaceTile(new(-1, 2), new(Color.Yellow, Shape.Circle))
                    .PlaceTile(new(0, 2), new(Color.Yellow, Shape.Clover));

        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(1, 0), new Tile(Color.Blue, Shape.Star)),
                new Placement(new(1, 1), new Tile(Color.Blue, Shape.Circle)),
                new Placement(new(1, 2), new Tile(Color.Blue, Shape.Clover)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(14,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    //Score ensuring that it doesn't count holes
    [Fact]
    public void TestPlaceScore2()
    {
        //         _
        //         _
        // RD  R8 RSt
        // GD
        // YD YCr YCv
        //         _
        Map testMap = new Map(new Tile(Color.Red, Shape.Star))
                    .PlaceTile(new(-1, 0), new(Color.Red, Shape.EightStar))
                    .PlaceTile(new(-2, 0), new(Color.Red, Shape.Diamond))
                    .PlaceTile(new(-2, 1), new(Color.Green, Shape.Diamond))
                    .PlaceTile(new(-2, 2), new(Color.Yellow, Shape.Diamond))
                    .PlaceTile(new(-1, 2), new(Color.Yellow, Shape.Circle))
                    .PlaceTile(new(0, 2), new(Color.Yellow, Shape.Clover));

        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(0, -1), new Tile(Color.Blue, Shape.Star)),
                new Placement(new(0, -2), new Tile(Color.Blue, Shape.Circle)),
                new Placement(new(0, 3), new Tile(Color.Blue, Shape.Clover)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(8,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    // Placing a single tile to get a Q
    [Fact]
    public void TestPlaceScore3()
    {
        //
        // BSq B8s BD BSt BCr _
        //
        Map testMap = new Map(new Tile(Color.Blue, Shape.Square))
                    .PlaceTile(new(1, 0), new(Color.Blue, Shape.EightStar))
                    .PlaceTile(new(2, 0), new(Color.Blue, Shape.Diamond))
                    .PlaceTile(new(3, 0), new(Color.Blue, Shape.Star))
                    .PlaceTile(new(4, 0), new(Color.Blue, Shape.Circle));


        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(5, 0), new Tile(Color.Blue, Shape.Clover)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(13,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    // 2 Qs, 1 tile placed, different directions
    [Fact]
    public void TestPlaceScore4()
    {
        //
        // BSq B8s BD BSt BCr  _
        //                YCr YCv
        //                    RCv
        //                    OCv
        //                    PCv
        //                    GCv
        Map testMap = new Map(new Tile(Color.Blue, Shape.Square))
                    .PlaceTile(new(1, 0), new(Color.Blue, Shape.EightStar))
                    .PlaceTile(new(2, 0), new(Color.Blue, Shape.Diamond))
                    .PlaceTile(new(3, 0), new(Color.Blue, Shape.Star))
                    .PlaceTile(new(4, 0), new(Color.Blue, Shape.Circle))
                    .PlaceTile(new(4, 1), new(Color.Yellow, Shape.Circle))
                    .PlaceTile(new(5, 1), new(Color.Yellow, Shape.Clover))
                    .PlaceTile(new(5, 2), new(Color.Red, Shape.Clover))
                    .PlaceTile(new(5, 3), new(Color.Orange, Shape.Clover))
                    .PlaceTile(new(5, 4), new(Color.Purple, Shape.Clover))
                    .PlaceTile(new(5, 5), new(Color.Green, Shape.Clover));
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(5, 0), new Tile(Color.Blue, Shape.Clover)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(25,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    // Single Q, player playes multiple of the tiles for it
    [Fact]
    public void TestPlaceScore5()
    {
        //
        // BSq B8s BD _  _  _
        //
        Map testMap = new Map(new Tile(Color.Blue, Shape.Square))
                    .PlaceTile(new(1, 0), new(Color.Blue, Shape.EightStar))
                    .PlaceTile(new(2, 0), new(Color.Blue, Shape.Diamond));

        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(3, 0), new Tile(Color.Blue, Shape.Star)),
                new Placement(new(4, 0), new Tile(Color.Blue, Shape.Circle)),
                new Placement(new(5, 0), new Tile(Color.Blue, Shape.Clover)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(15,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    // 2 Seprate Qs, both horizonatal
    [Fact]
    public void TestPlaceScore6()
    {
        //
        // BSq B8s BD BSt BCr _
        // YSq
        // YSt RSt OSt BSt PSt _
        Map testMap = new Map(new Tile(Color.Blue, Shape.Square))
                    .PlaceTile(new(1, 0), new(Color.Blue, Shape.EightStar))
                    .PlaceTile(new(2, 0), new(Color.Blue, Shape.Diamond))
                    .PlaceTile(new(3, 0), new(Color.Blue, Shape.Star))
                    .PlaceTile(new(4, 0), new(Color.Blue, Shape.Circle))
                    .PlaceTile(new(0, 1), new(Color.Yellow, Shape.Square))
                    .PlaceTile(new(0, 2), new(Color.Yellow, Shape.Star))
                    .PlaceTile(new(1, 2), new(Color.Red, Shape.Star))
                    .PlaceTile(new(2, 2), new(Color.Orange, Shape.Star))
                    .PlaceTile(new(3, 2), new(Color.Blue, Shape.Star))
                    .PlaceTile(new(4, 2), new(Color.Purple, Shape.Star));

        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(5, 0), new Tile(Color.Blue, Shape.Clover)),
                new Placement(new(5, 2), new Tile(Color.Green, Shape.Star)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(26,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    // Not a Q because row becomes too long
    [Fact]
    public void TestPlaceScore7()
    {
        //
        // BSq B8s BD BSt BCr _ _
        //
        Map testMap = new Map(new Tile(Color.Blue, Shape.Square))
                    .PlaceTile(new(1, 0), new(Color.Blue, Shape.EightStar))
                    .PlaceTile(new(2, 0), new(Color.Blue, Shape.Diamond))
                    .PlaceTile(new(3, 0), new(Color.Blue, Shape.Star))
                    .PlaceTile(new(4, 0), new(Color.Blue, Shape.Circle));


        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(5, 0), new Tile(Color.Blue, Shape.Clover)),
                new Placement(new(6, 0), new Tile(Color.Blue, Shape.Star)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(9,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }

    // 3 Qs, 2 tile placed, different directions
    [Fact]
    public void TestPlaceScore8()
    {
        //
        // BSq B8s BD BCv BCr  _
        // GSq G8s GD GCv GCr  _
        //                GSt RSt
        //                    OSt
        //                    PSt
        //                    YSt
        Map testMap = new Map(new Tile(Color.Blue, Shape.Square))
                        .PlaceTile(new(1, 0), new(Color.Blue, Shape.EightStar))
                        .PlaceTile(new(2, 0), new(Color.Blue, Shape.Diamond))
                        .PlaceTile(new(3, 0), new(Color.Blue, Shape.Clover))
                        .PlaceTile(new(4, 0), new(Color.Blue, Shape.Circle))
                        .PlaceTile(new(0, 1), new(Color.Green, Shape.Square))
                        .PlaceTile(new(1, 1), new(Color.Green, Shape.EightStar))
                        .PlaceTile(new(2, 1), new(Color.Green, Shape.Diamond))
                        .PlaceTile(new(3, 1), new(Color.Green, Shape.Clover))
                        .PlaceTile(new(4, 1), new(Color.Green, Shape.Circle))
                        .PlaceTile(new(4, 2), new(Color.Green, Shape.Star))
                        .PlaceTile(new(5, 2), new(Color.Red, Shape.Star))
                        .PlaceTile(new(5, 3), new(Color.Orange, Shape.Star))
                        .PlaceTile(new(5, 4), new(Color.Purple, Shape.Star))
                        .PlaceTile(new(5, 5), new(Color.Yellow, Shape.Star));
        var state = new GameState.Builder(TileFixture())
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build(testMap);
        var action = new Place(new [] {
                new Placement(new(5, 0), new Tile(Color.Blue, Shape.Star)),
                new Placement(new(5, 1), new Tile(Color.Green, Shape.Star)),
            });
        Assert.True(action.IsValid(state.PublicInfo()));
        Assert.Equal(38,
                     action.GetScore(new ScoreConfiguration(6, 6), state.PublicInfo()));
    }
}
