namespace Q.Common;

/// A cycle over player type T, responsible for tracking the current player
/// taking a turn and the start of new rounds.
public interface ICycle<T>
{
    /// Gets the players other than the active player, in turn order where the
    /// first player is the player that directly succeeds the current player.
    public IEnumerable<T> OtherPlayers();

    /// The player of the current term
    public T ActivePlayer();

    /// Advances the turn to the next player
    public void BumpPlayer();

    /// Advances the turn to the next player, while removing the active player
    public void KickPlayer();

    /// Is the cycle at the beginning of a round?
    public bool RoundChange();

    /// Does the cycle have players other than the current player?
    public bool HasOtherPlayers();

    /// Number of players in the cycle, including the current player
    public int Count();

    /// Enumerates over all players, including the current player
    public IEnumerable<T> AllPlayers();
}

/// A "lens" into a cycle. It wraps a delegate ICycle object to transform the
/// latter's data via a user-provided function. Advancing this cycle advances
/// the delegate ICycle and advancing the delegate ICycle advances this cycle.
public class LensCycle<T, U> : ICycle<U>
{
    private ICycle<T> _inner;
    private Func<T, U> _map;

    /// Creates a "lens" into the inner cycle, using a function to "map" the
    /// outer data to the inner data.
    public LensCycle(ICycle<T> inner, Func<T, U> map)
    {
        _inner = inner;
        _map = map;
    }

    public IEnumerable<U> OtherPlayers()
    {
        return _inner.OtherPlayers().Select(_map);
    }

    public U ActivePlayer()
    {
        return _map(_inner.ActivePlayer());
    }

    public void BumpPlayer()
    {
        _inner.BumpPlayer();
    }

    public void KickPlayer()
    {
        _inner.KickPlayer();
    }

    public bool RoundChange()
    {
        return _inner.RoundChange();
    }

    public bool HasOtherPlayers()
    {
        return _inner.HasOtherPlayers();
    }

    public ICycle<T> Inner()
    {
        return _inner;
    }

    public int Count()
    {
        return _inner.Count();
    }

    public IEnumerable<U> AllPlayers()
    {
        return _inner.AllPlayers().Select(_map);
    }
}

/// Maintains the player turn order and the start and end of a round.
public class Cycle<T> : ICycle<T>
{
    private Queue<T> _curRound;
    private Queue<T> _nextRound;
    private T _activePlayer;
    private bool _isActivePlayerKicked = false;

    private bool _beginOfRound = false;

    /// Takes ownership of the queue.
    public Cycle(Queue<T> players)
    {
        _curRound = players;
        _nextRound = new Queue<T>();
        if(_curRound.Any()) {
            _activePlayer = _curRound.Dequeue();
        }
        else {
            _isActivePlayerKicked = true;
        }
    }

    public Cycle(IEnumerable<T> players)
    {
        _curRound = new Queue<T>(players);
        _nextRound = new Queue<T>();
        if(_curRound.Any()) {
            _activePlayer = _curRound.Dequeue();
        }
        else {
            _isActivePlayerKicked = true;
        }
    }

    /// Gets the players other than the active player, in turn order where the
    /// first player is the player that directly succeeds the current player.
    public IEnumerable<T> OtherPlayers()
    {
        List<T> players = new List<T>();
        foreach (var player in _curRound)
        {
            players.Add(player);
        }
        foreach (var player in _nextRound)
        {
            players.Add(player);
        }
        return players;
    }

    /// The player of the current term
    public T ActivePlayer()
    {
        return _activePlayer;
    }

    /// Advances the turn to the next player
    public void BumpPlayer()
    {
        _nextRound.Enqueue(_activePlayer);
        if (!_curRound.Any())
        {
            _beginOfRound = true;
            (_curRound, _nextRound) = (_nextRound, _curRound);
        }
        else {_beginOfRound = false;}
        _activePlayer = _curRound.Dequeue();
    }

    /// Advances the turn to the next player, while removing the active player
    public void KickPlayer()
    {
        if(!HasOtherPlayers())
        {
            _isActivePlayerKicked = true;
            _beginOfRound = true;
            return;
        }
        if (!_curRound.Any())
        {
            _beginOfRound = true;
            (_curRound, _nextRound) = (_nextRound, _curRound);
        }
        else {_beginOfRound = false;}
        _activePlayer = _curRound.Dequeue();
    }

    /// Is the cycle at the point where the round will be reset
    public bool RoundChange()
    {
        return _beginOfRound;
    }

    /// Does the cycle have players other than the current player?
    public bool HasOtherPlayers()
    {
        return _nextRound.Any() || _curRound.Any();
    }

    public IEnumerable<T> AllPlayers()
    {
        if (!_isActivePlayerKicked)
        {
            yield return _activePlayer;
        }
        foreach (var player in _curRound)
        {
            yield return player;
        }
        foreach (var player in _nextRound)
        {
            yield return player;
        }
    }

    public int Count()
    {
        if(_isActivePlayerKicked)
        {
            return 0;
        }
        return _nextRound.Count + _curRound.Count + 1;
    }
}
