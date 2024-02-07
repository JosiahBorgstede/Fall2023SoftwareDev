using Q.Common;

namespace Other;

public class ObserverUnitTests
{
    [Fact]
    public void TestSaveToFile()
    {
        GuiObserver observer = new(600, 600);
        GameState state = new GameState.Builder()
            .AddPlayer("foo")
            .AddPlayer("foo")
            .Build();
        observer.AddState(state);
        observer.SaveAsJState("jstate.txt", 0);
    }

}
