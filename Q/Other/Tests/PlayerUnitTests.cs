using Moq;
using Q.Common;
using Q.Player;
using Xunit.Sdk;

namespace Other;

public class PlayerUnitTests
{
    [Fact]
    public void PlayerTest1()
    {
        Mock<IStrategy> strat = new();
        PlayerTurnInfo mockTurnInfo = new(
            new(new(Color.Red, Shape.Star)),
            new(new List<Tile>(){new(Color.Red, Shape.EightStar)}, 10, "bob"),
            new List<PublicPlayerInfo>(),
            20);
        strat.Setup(x => x.ApplyStrategy(It.IsAny<PlayerTurnInfo>()))
            .Returns(new Pass());
        IPlayer player = new InHousePlayer("bob", strat.Object);
        Assert.Equal("bob", player.name());
        Assert.IsType<Pass>(player.takeTurn(mockTurnInfo));
        strat.Verify(x => x.ApplyStrategy(mockTurnInfo));
    }

}