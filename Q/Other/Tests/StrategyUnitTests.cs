using Q.Common;
using Q.Player;

namespace Other;

public class StrategyUnitTests
{
    [Fact]
    public void TestNoTiles()
    {
        Map map = new Map(new(Color.Blue, Shape.Clover))
            .PlaceTile(new(1, 0), new(Color.Blue, Shape.Star))
            .PlaceTile(new(0, 1), new(Color.Red, Shape.Clover));
        PlayerTurnInfo info = new(
            map,
            new PlayerState(
                new Tile[] {
                    new(Color.Green, Shape.Square)
                }, 0, "foo"),
            new PublicPlayerInfo[] {
                new(0)
            },
            0
        );
        Assert.IsType<Pass>(new Dag().ApplyStrategy(info));
        Assert.IsType<SingleAction.Pass>(new Dag().ApplyStrategyOnce(info));
        Assert.IsType<Pass>(new Ldasg().ApplyStrategy(info));
        Assert.IsType<SingleAction.Pass>(new Ldasg().ApplyStrategyOnce(info));
    }

    [Fact]
    public void TestOneTile()
    {
        Map map = new Map(new(Color.Blue, Shape.Clover))
            .PlaceTile(new(1, 0), new(Color.Blue, Shape.Star))
            .PlaceTile(new(0, 1), new(Color.Red, Shape.Clover));
        PlayerTurnInfo info = new(
            map,
            new PlayerState(
                new Tile[] {
                    new(Color.Green, Shape.Square)
                }, 0, "foo"),
            new PublicPlayerInfo[] {
                new(0)
            },
            1
        );
        Assert.IsType<Exchange>(new Dag().ApplyStrategy(info));
        Assert.IsType<SingleAction.Replace>(new Dag().ApplyStrategyOnce(info));
        Assert.IsType<Exchange>(new Ldasg().ApplyStrategy(info));
        Assert.IsType<SingleAction.Replace>(new Ldasg().ApplyStrategyOnce(info));
    }

    [Fact]
    public void TestPlace1()
    {
        Map map = new Map(new(Color.Red, Shape.Star))
            .PlaceTile(new(0, 1), new(Color.Green, Shape.Star))
            .PlaceTile(new(1, 1), new(Color.Purple, Shape.Star));
        var tiles = new Tile[] {
            new(Color.Purple, Shape.Star)
        };
        PlayerTurnInfo info = new(
            map,
            new PlayerState(tiles, 0, "foo"),
            new PublicPlayerInfo[] {
                new(0)
            },
            0
        );
        Assert.IsType<Place>(new Dag().ApplyStrategy(info));
        Assert.IsType<Place>(new Ldasg().ApplyStrategy(info));
        Assert.Equal(new Placement(new(0, -1), new(Color.Purple, Shape.Star)),
                     ((SingleAction.OnePlacement)new Dag().ApplyStrategyOnce(info)).Placement);
        Assert.Equal(new Placement(new(1, 0), new(Color.Purple, Shape.Star)),
                     ((SingleAction.OnePlacement)new Ldasg().ApplyStrategyOnce(info)).Placement);
    }
    [Fact]
    public void TestPlace2()
    {
        Map map = new Map(new(Color.Red, Shape.Star))
            .PlaceTile(new(0, 1), new(Color.Green, Shape.Star))
            .PlaceTile(new(1, 1), new(Color.Purple, Shape.Star))
            .PlaceTile(new(0, 2), new(Color.Green, Shape.Star))
            .PlaceTile(new(2, 1), new(Color.Orange, Shape.Star))
            .PlaceTile(new(2, 2), new(Color.Green, Shape.Star));
        var tiles = new Tile[] {
            new(Color.Purple, Shape.Star)
        };
        PlayerTurnInfo info = new(
            map,
            new PlayerState(tiles, 0, "foo"),
            new PublicPlayerInfo[] {
                new(0)
            },
            0
        );
        Assert.IsType<Place>(new Dag().ApplyStrategy(info));
        Assert.IsType<Place>(new Ldasg().ApplyStrategy(info));
        Assert.Equal(new Placement(new(0, -1), new(Color.Purple, Shape.Star)),
                     ((SingleAction.OnePlacement)new Dag().ApplyStrategyOnce(info)).Placement);
        Assert.Equal(new Placement(new(1, 2), new(Color.Purple, Shape.Star)),
                     ((SingleAction.OnePlacement)new Ldasg().ApplyStrategyOnce(info)).Placement);
    }

    [Fact]
    public void TestPlace3()
    {
        Map map = new Map(new(Color.Red, Shape.Star))
            .PlaceTile(new(0, 1), new(Color.Green, Shape.Star))
            .PlaceTile(new(1, 1), new(Color.Purple, Shape.Star))
            .PlaceTile(new(0, 2), new(Color.Green, Shape.Star))
            .PlaceTile(new(2, 1), new(Color.Orange, Shape.Star))
            .PlaceTile(new(2, 2), new(Color.Green, Shape.Star));
        var tiles = new Tile[] {
            new(Color.Purple, Shape.Star),
            new(Color.Orange, Shape.Star),
        };
        PlayerTurnInfo info = new(
            map,
            new PlayerState(tiles, 0, "foo"),
            new PublicPlayerInfo[] {
                new(0)
            },
            0
        );
        Assert.IsType<Place>(new Dag().ApplyStrategy(info));
        Assert.IsType<Place>(new Ldasg().ApplyStrategy(info));
        Assert.Equal(new Placement(new(0, -1), new(Color.Orange, Shape.Star)),
                     ((SingleAction.OnePlacement)new Dag().ApplyStrategyOnce(info)).Placement);
        Assert.Equal(new Placement(new(1, 2), new(Color.Orange, Shape.Star)),
                     ((SingleAction.OnePlacement)new Ldasg().ApplyStrategyOnce(info)).Placement);
    }

    [Fact]
    public void TestPlace4()
    {
        Map map = new Map(new(Color.Orange, Shape.EightStar))
            .PlaceTile(new(0, 1), new(Color.Purple, Shape.EightStar))
            .PlaceTile(new(0, 2), new(Color.Purple, Shape.Star))
            .PlaceTile(new(1, 2), new(Color.Blue, Shape.Square))
            .PlaceTile(new(1, 3), new(Color.Blue, Shape.Square))
            .PlaceTile(new(1, 4), new(Color.Blue, Shape.Square))
            .PlaceTile(new(2, 4), new(Color.Blue, Shape.Square))
            .PlaceTile(new(3, 4), new(Color.Blue, Shape.Square))
            .PlaceTile(new(3, 3), new(Color.Blue, Shape.Circle))
            .PlaceTile(new(3, 2), new(Color.Orange, Shape.Clover));
        var tiles = new Tile[] {
            new(Color.Orange, Shape.Square),
        };
        PlayerTurnInfo info = new(
            map,
            new PlayerState(tiles, 0, "foo"),
            new PublicPlayerInfo[] {
                new(0)
            },
            0
        );
        Assert.IsType<Place>(new Dag().ApplyStrategy(info));
        Assert.IsType<Place>(new Ldasg().ApplyStrategy(info));
        Assert.Equal(new Placement(new(0, -1), new(Color.Orange, Shape.Square)),
                     ((SingleAction.OnePlacement)new Dag().ApplyStrategyOnce(info)).Placement);
        Assert.Equal(new Placement(new(0, -1), new(Color.Orange, Shape.Square)),
                     ((SingleAction.OnePlacement)new Ldasg().ApplyStrategyOnce(info)).Placement);
    }
}
