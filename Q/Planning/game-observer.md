# Memorandum

To: CEOs

From: Alan Hu and Josiah Borgstede

CC: Matthias Felleisen

Date: October 26, 2023

Overload the signature of the referee function to the following:

    (IEnumerable<IPlayer>, IEnumerable<IPlayer>)
    referee(IEnumerable<IPlayer> players, IObserver observer)

The referee will call `Update` method of the `observer` at the beginning of
each turn.

The `IObservers` interface contains operation for adding and removing
listeners and forwarding game states to all listeners.

    interface IObservers
        IListener AddListener();
        // Subscribes a listener to the game

        void RemoveObserver(IListener);
        // Unsubscribes a listener from the game

        void Update(SpectatorInfo info);
        // Forwards the SpectatorInfo to all subscribed listeners

        void GameEnd(SpectatorInfo info)
        // Forwards the SpectatorInfo to all subscribed listeners,
        // additionally notifying them of end of game

The `IListener` interface should be implemented by clients to respond to
changes in the game state. The `update` method will be called at the beginning
of each turn.

    interface IListener
        void Update(SpectatorInfo info);
        // Acts on the SpectatorInfo

        void GameEnd(SpectatorInfo info)
        // Acts on the notification that the game has ended on the given game
        // state

<img align="right" src="Media/ObserverSequence.png" alt="drawing" height="600"/>

The listeners are added to the observer. The observer is passed to the referee
function along with a list of players. Then, the referee begins normal play.
When the referee sends the public player knowledge to the active player, it also
sends the public spectator knowledge to the observer. The observer forwards this
knowledge to each of its listeners. This process repeats until the game is
over, at which point the referee calls the win method of the players and the
GameEnd method of the observer, passing the final public observer knowledge.
The observer then calls the GameEnd method of each listener.