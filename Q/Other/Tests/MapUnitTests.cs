using Q.Common;

namespace Other;

public class MapUnitTests
{
    [Fact]
    public void TestPlaceTileValid()
    {
        Map testMap = new Map(new Tile(Color.Green, Shape.Star));
        Assert.Equal(new Tile(Color.Green, Shape.Star),
                     testMap.GetTile(new Coordinate(0, 0)));
        Assert.Null(testMap.GetTile(new Coordinate(0, 1)));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        Assert.Equal(new Tile(Color.Green, Shape.Star),
                     testMap.GetTile(new Coordinate(0, 0)));
        Assert.Equal(new Tile(Color.Red, Shape.Diamond),
                     testMap.GetTile(new Coordinate(0, 1)));
    }

    [Fact]
    public void TestPlaceTileInvalid()
    {
        Map testMap = new Map(new Tile(Color.Green, Shape.Star));
        Assert.Equal(new Tile(Color.Green, Shape.Star),
                     testMap.GetTile(new Coordinate(0, 0)));
        Assert.Null(testMap.GetTile(new Coordinate(1, 1)));
        Assert.Throws<InvalidOperationException>(
            () =>testMap.PlaceTile(new Coordinate(1, 1),
                                   new Tile(Color.Red, Shape.Diamond)));
        Assert.Equal(new Tile(Color.Green, Shape.Star),
                     testMap.GetTile(new Coordinate(0, 0)));
        Assert.Null(testMap.GetTile(new Coordinate(1, 1)));
    }

    [Fact]
    public void TestValidPlacements1()
    {
        Map testMap = new Map(new Tile(Color.Green, Shape.Diamond));
        Assert.Equal(new[] {
                new Coordinate(0, -1),
                new Coordinate(-1, 0),
                new Coordinate(1, 0),
                new Coordinate(0, 1),
            },
            testMap.ValidPlacements(new Tile(Color.Green, Shape.Star)));
    }

    [Fact]
    public void TestValidPlacements2()
    {
        Map testMap = new Map(new Tile(Color.Green, Shape.Diamond));
        Assert.Equal(new[] {
                new Coordinate(0, -1),
                new Coordinate(-1, 0),
                new Coordinate(1, 0),
                new Coordinate(0, 1),
            },
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Diamond)));
    }

    [Fact]
    public void TestValidPlacements3()
    {
        Map testMap = new Map(new Tile(Color.Green, Shape.Diamond));
        Assert.Equal(Enumerable.Empty<Coordinate>(),
                     testMap.ValidPlacements(new Tile(Color.Red, Shape.Star)));
    }

    [Fact]
    public void TestValidPlacements4()
    {
        // BD
        // RD RC
        Map testMap = new Map(new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 1),
                                    new Tile(Color.Red, Shape.Clover));

        Assert.Equal(new [] {
                new Coordinate(0, -1),
                new Coordinate(-1, 0),
                new Coordinate(1, 0),
                new Coordinate(-1, 1),
                new Coordinate(2, 1),
                new Coordinate(0, 2),
                new Coordinate(1, 2),
            },
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Diamond)));

        Assert.Equal(new [] {
                new Coordinate(-1, 1),
                new Coordinate(2, 1),
                new Coordinate(0, 2),
                new Coordinate(1, 2),
            },
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Clover)));

        Assert.Equal(new [] {
                new Coordinate(2, 1),
                new Coordinate(1, 2),
            },
            testMap.ValidPlacements(new Tile(Color.Green, Shape.Clover)));

        Assert.Equal(
            Enumerable.Empty<Coordinate>(),
            testMap.ValidPlacements(new Tile(Color.Green, Shape.Star)));
    }

    [Fact]
    public void TestValidPlacementAlongRowMatchingShapeWithGap()
    {
        //
        //
        // BD       OS
        // BC RC YC OC
        //
        Map testMap = new Map(new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Blue, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(1, 1),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 1),
                                    new Tile(Color.Yellow, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(3, 1),
                                    new Tile(Color.Orange, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(3, 0),
                                    new Tile(Color.Orange, Shape.Star));

        Assert.Equal(new [] {
                new Coordinate(0, -1),
                new Coordinate(-1, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 2),
            },
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Diamond)));
    }

    [Fact]
    public void TestValidPlacementAlongColumnMatchingShapeWithGap()
    {
        // BC BD
        // RC
        // YC
        // OC OS
        Map testMap = new Map(new Tile(Color.Blue, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Yellow, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(0, 3),
                                    new Tile(Color.Orange, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 3),
                                    new Tile(Color.Orange, Shape.Star));

        Assert.Equal(new [] {
                new Coordinate(1, -1),
                new Coordinate(2, 0),
                new Coordinate(-1, 1),
                new Coordinate(1, 1),
            },
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Diamond)));
    }

    [Fact]
    public void TestValidPlacementAlongRowMatchingColorWithGap()
    {
        // BD        OSq
        // RD RC RSt RSq
        Map testMap = new Map(new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 1),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 1),
                                    new Tile(Color.Red, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(3, 1),
                                    new Tile(Color.Red, Shape.Square));
        testMap = testMap.PlaceTile(new Coordinate(3, 0),
                                    new Tile(Color.Orange, Shape.Square));

        Assert.Equal(new [] {
                new Coordinate(0, -1),
                new Coordinate(-1, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 2),
            },
            testMap.ValidPlacements(new Tile(Color.Blue, Shape.Clover)));
    }

    [Fact]
    public void TestValidPlacementAlongColumnMatchingColorWithGap()
    {
        // BC  RC
        // BD
        // BSq
        // BSt OSt
        Map testMap = new Map(new Tile(Color.Blue, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Blue, Shape.Square));
        testMap = testMap.PlaceTile(new Coordinate(0, 3),
                                    new Tile(Color.Blue, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(1, 3),
                                    new Tile(Color.Orange, Shape.Star));

        Assert.Equal(new [] {
                new Coordinate(1, -1),
                new Coordinate(2, 0),
                new Coordinate(-1, 1),
                new Coordinate(1, 1),
            },
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Diamond)));
    }

    [Fact]
    public void TestTwoDifferentValidCoordinatesWithManyTiles()
    {
        //                                  GSq
        //                            ___   BSq    YSq
        //    PD    RD    YD    GD          BCir
        //    PSq               GSt   OSt   BSt    RSt
        //    PSt                           YSt
        //__  PCl
        //    PCir  YCir
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Purple, Shape.Square));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(0, 3),
                                    new Tile(Color.Purple, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(0, 4),
                                    new Tile(Color.Purple, Shape.Circle));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Yellow, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(3, 0),
                                    new Tile(Color.Green, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 4),
                                    new Tile(Color.Yellow, Shape.Circle));
        testMap = testMap.PlaceTile(new Coordinate(3, 1),
                                    new Tile(Color.Green, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(4, 1),
                                    new Tile(Color.Orange, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(5, 1),
                                    new Tile(Color.Blue, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(5, 0),
                                    new Tile(Color.Blue, Shape.Circle));
        testMap = testMap.PlaceTile(new Coordinate(5, -1),
                                    new Tile(Color.Blue, Shape.Square));
        testMap = testMap.PlaceTile(new Coordinate(6, -1),
                                    new Tile(Color.Yellow, Shape.Square));
        testMap = testMap.PlaceTile(new Coordinate(5, 2),
                                    new Tile(Color.Yellow, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(5, -2),
                                    new Tile(Color.Green, Shape.Square));
        testMap = testMap.PlaceTile(new Coordinate(6, 1),
                                    new Tile(Color.Red, Shape.Star));

        Assert.Equal(new[] {
                new Coordinate(4, -1),
                new Coordinate(-1, 3),
            },
            testMap.ValidPlacements(new Tile(Color.Blue, Shape.Clover)));
    }

    [Fact]
    public void TestOneValidCoordinate()
    {
        //   BD
        //   PD PSt RSt
        //   RD
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, -1),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1,0),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Red, Shape.Star));
        Assert.Equal(new[] {
                new Coordinate(-1, 0),
            },
            testMap.ValidPlacements(new Tile(Color.Purple, Shape.Square)));
    }

    [Fact]
    public void TestNoValidCoordinate()
    {
        //   BD
        //   PD PSt RSt
        //   RD
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, -1),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Red, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(-1, 0),
                                    new Tile(Color.Green, Shape.Diamond));

        Assert.Equal(Enumerable.Empty<Coordinate>(),
                     testMap.ValidPlacements(
                         new Tile(Color.Purple, Shape.Square)));
    }

    [Fact]
    public void TestCoordinateValidityInsideBox()
    {
        //   PD PSt RSt
        //   RD     RCl
        //   BD BSt BCl
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Yellow, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(1, 2),
                                    new Tile(Color.Blue, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 2),
                                    new Tile(Color.Blue, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 1),
                                    new Tile(Color.Red, Shape.Clover));

        Assert.DoesNotContain(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Purple, Shape.Square)));
        Assert.Contains(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Star)));
    }

    [Fact]
    public void TestCoordinateValidityInsideRightOpenBox()
    {
        //   PD PSt Pcl
        //   RD
        //   BD BSt BCl
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Purple, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(1, 2),
                                    new Tile(Color.Blue, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 2),
                                    new Tile(Color.Blue, Shape.Clover));

        Assert.DoesNotContain(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Purple, Shape.Square)));

        Assert.Contains(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Star)));
    }

    [Fact]
    public void TestCoordinateValidityInsideBottomOpenBox()
    {
        //   PD     PCl
        //   RD     RCl
        //   BD BSt BCl
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 2),
                                    new Tile(Color.Blue, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 2),
                                    new Tile(Color.Blue, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 1),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Purple, Shape.Clover));


        Assert.DoesNotContain(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Purple, Shape.Square)));

        Assert.Contains(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Star)));
    }

    [Fact]
    public void TestCoordinateValidityInsideTopOpenBox()
    {
        //   PD PSt PCl
        //   RD     RCl
        //   BD     BCl
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 1),
                                    new Tile(Color.Red, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Blue, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Purple, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 1),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 2),
                                    new Tile(Color.Blue, Shape.Clover));


        Assert.DoesNotContain(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Purple, Shape.Square)));

        Assert.Contains(new Coordinate(1, 1),
            testMap.ValidPlacements(new Tile(Color.Red, Shape.Star)));
    }

    [Fact]
    public void TestCoordinateValidityInsideLeftOpenBox()
    {
        //   PD PSt PCl
        //          RCl
        //   BD BSt BCl
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Purple, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(2, 0),
                                    new Tile(Color.Purple, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 1),
                                    new Tile(Color.Red, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(2, 2),
                                    new Tile(Color.Blue, Shape.Clover));
        testMap = testMap.PlaceTile(new Coordinate(1, 2),
                          new Tile(Color.Blue, Shape.Star));
        testMap = testMap.PlaceTile(new Coordinate(0, 2),
                                    new Tile(Color.Blue, Shape.Diamond));

        Assert.DoesNotContain(new Coordinate(1, 1),
                              testMap.ValidPlacements(new Tile(Color.Purple, Shape.Square)));

       Assert.Contains(new Coordinate(1, 1),
                       testMap.ValidPlacements(new Tile(Color.Red, Shape.Star)));
    }

    [Fact]
    public void TestThatTilesCannotOverlap()
    {
        Map testMap = new Map(new Tile(Color.Purple, Shape.Diamond));

        Assert.Null(testMap.GetTile(new Coordinate(1, 0)));
        testMap = testMap.PlaceTile(new Coordinate(1, 0),
                          new Tile(Color.Purple, Shape.Star));
        Assert.Equal(new Tile(Color.Purple, Shape.Star),
                     testMap.GetTile(new Coordinate(1, 0)));
        Assert.Throws<InvalidOperationException>(
            () => testMap.PlaceTile(new Coordinate(1, 0),
                                    new Tile(Color.Red, Shape.Star)));
        Assert.Equal(new Tile(Color.Purple, Shape.Star),
                     testMap.GetTile(new Coordinate(1, 0)));
    }


    [Fact]
    public void TestNeighborsEmpty()
    {
        var neighbor = new Neighbors {
            Up = null,
            Down = null,
            Left = null,
            Right = null
        };
        Assert.False(neighbor.HasTile());
    }

    [Fact]
    public void TestNeighborsNonempty()
    {
        var neighbor1 = new Neighbors {
            Up = new Tile(Color.Green, Shape.Star),
            Down = null,
            Left = null,
            Right = null
        };
        Assert.True(neighbor1.HasTile());

        var neighbor2 = new Neighbors {
            Up = null,
            Down = new Tile(Color.Green, Shape.Star),
            Left = null,
            Right = null
        };
        Assert.True(neighbor2.HasTile());

        var neighbor3 = new Neighbors {
            Up = null,
            Down = null,
            Left = new Tile(Color.Green, Shape.Star),
            Right = null
        };
        Assert.True(neighbor3.HasTile());

        var neighbor4 = new Neighbors {
            Up = null,
            Down = null,
            Left = null,
            Right = new Tile(Color.Green, Shape.Star)
        };
        Assert.True(neighbor4.HasTile());

        var neighbor5 = new Neighbors {
            Up = new Tile(Color.Red, Shape.Diamond),
            Down = null,
            Left = null,
            Right = new Tile(Color.Green, Shape.Star)
        };
        Assert.True(neighbor5.HasTile());
    }
}
