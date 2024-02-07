using Q.Common;
using Q.Player;

namespace Q.Referee;

public class RefereeDriver
{
    private ScoreConfiguration scoreConfiguration;

    private IObserver? observer;

    private IGameState? _initialState;
    private TimeSpan timePerAction;

    public RefereeDriver(RefereeConfiguration configuration)
    {
        scoreConfiguration = configuration.scoreConfiguration;
        if(configuration.ObserverEnabled)
        {
            observer = new GuiObserver(600, 600);
            Renderer renderer = new(600, 600);
            Thread tread = new Thread(new ThreadStart(() => renderer.RenderToInteractiveWindow(observer as GuiObserver)));
            tread.Start();
        }
        timePerAction = configuration.timePerPlayerAction;
        _initialState = configuration.state;
    }

    private bool RunTurn(IPlayer current, IGameState gameState)
    {
        IAction currentAction = new Pass();
        if(!SafeCallMethod(() => currentAction = current.takeTurn(gameState.PublicInfo())))
        {
            return false;
        }
        if (!currentAction.IsValid(gameState.PublicInfo())) {return false;}
        gameState.ActivePlayer().Score +=
            currentAction.GetScore(scoreConfiguration,
                                   gameState.PublicInfo());
        var delta = currentAction.Commit(gameState);
        if(!gameState.IsGameOver && currentAction.GetType() != typeof(Pass))
        {
            return SafeCallMethod(() => current.newTiles(delta));
        }
        return true;
    }

    //Runs an entire round of the game, returning false if the game is over.
    private bool RunRound(IGameState gameState,
                          ICycle<(PlayerState, IPlayer)> playerCycle,
                          IList<IPlayer> kicked)
    {
        gameState.NextRound();
        do {
            (_, var current) = playerCycle.ActivePlayer();
            observer?.AddState(gameState);
            if (RunTurn(current, gameState))
            {
                gameState.BumpPlayer();
            }
            else
            {
                gameState.KickPlayer();
                kicked.Add(current);
            }
            if(gameState.IsGameOver){return false;}
        }
        while(!playerCycle.RoundChange() && playerCycle.Count() > 0);
        return gameState.WasMapUpdated();
    }

    private (IEnumerable<IPlayer>, IEnumerable<IPlayer>)
        WinnersAndLosers(IEnumerable<(PlayerState, IPlayer)> pairs)
    {
        if (!pairs.Any())
        {
            return (Enumerable.Empty<IPlayer>(), Enumerable.Empty<IPlayer>());
        }
        int winningScore = pairs.Select(p => p.Item1.Score).Max();
        return (
            from pair in pairs
            where pair.Item1.Score == winningScore
            select pair.Item2,
            from pair in pairs
            where pair.Item1.Score != winningScore
            select pair.Item2);
    }

    private ICycle<(PlayerState, IPlayer)>
        WrapGameState(IEnumerable<IPlayer> players, IGameState gameState)
    {
        ICycle<PlayerState> oldCycle = gameState.Cycle();
        Queue<(PlayerState, IPlayer)> queue = new();
        if (oldCycle.Count() != players.Count())
        {
            throw new ArgumentException("Lengths don't match");
        }
        foreach ((var player, var iplayer) in
                 oldCycle.AllPlayers().Zip(players))
        {
            if(player.Name != iplayer.name())
            {
                throw new ArgumentException("name mismatch for players");
            }
            queue.Enqueue((player, iplayer));
        }
        var cycle = new Cycle<(PlayerState, IPlayer)>(queue);
        var lensCycle =
            new LensCycle<(PlayerState, IPlayer), PlayerState>(
                cycle, p => p.Item1);
        gameState.ReplaceCycle(lensCycle);
        return cycle;
    }

    /// Handles safety and logical and dos errors
    public (IEnumerable<string>, IEnumerable<string>) RefereeFromState(
        IEnumerable<IPlayer> players, IGameState gameState)
    {
        ICycle<(PlayerState, IPlayer)> cycle = WrapGameState(players, gameState);
        List<IPlayer> kicked = SetupPlayers(cycle, gameState).ToList();
        while (cycle.Count() > 0 && RunRound(gameState, cycle, kicked));
        (var winners, var losers) = WinnersAndLosers(cycle.AllPlayers());
        foreach (var player in winners)
        {
            if(!SafeCallMethod(() => player.win(true)))
            {
                kicked.Add(player);
                var newWis = winners.ToList();
                newWis.Remove(player);
                winners = newWis;
            }
        }
        foreach (var player in losers)
        {
            if(!SafeCallMethod(() => player.win(false)))
            {
                kicked.Add(player);
            }
        }
        observer?.AddWinningState(gameState);
        return (winners.Select(p => p.name()).Order(StringComparer.Ordinal),
                kicked.Select(p => p.name()));
    }

    public (IEnumerable<string>, IEnumerable<string>)
        Referee(IEnumerable<IPlayer> iplayers)
    {
        if(_initialState != null)
        {
            return RefereeFromState(iplayers, _initialState);
        }

        var builder = new GameState.Builder()
            .RandomizeTiles();

        foreach (var player in iplayers)
        {
            builder.AddPlayer(player.name());
        }

        return RefereeFromState(iplayers, builder.Build());
    }

    /// calls setup on all of the players, returning a list of those who faulted
    private IEnumerable<IPlayer> SetupPlayers(
        ICycle<(PlayerState, IPlayer)> cycle, IGameState state)
    {
        List<IPlayer> failedSetup = new();
        do {
            (var player, var current) = cycle.ActivePlayer();
            if(SafeCallMethod(() => current.setup(state.PublicInfo(), player.GetTiles())))
            {
                state.BumpPlayer();
            }
            else
            {
                state.KickPlayer();
                failedSetup.Add(current);
            }
        } while(!cycle.RoundChange());
        return failedSetup;
    }

    //Returns true if toRun finishes in timePerAction, if toRun throws or takes
    //to long, returns false.
    private bool SafeCallMethod(Action toRun)
    {
        var result = Task.Run(toRun);
        try
        {
            return result.Wait(timePerAction);
        }
        catch
        {
            return false;
        }
    }
}
