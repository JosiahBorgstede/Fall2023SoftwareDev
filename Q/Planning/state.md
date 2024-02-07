# Memorandum

To: CEOs

From: Alan Hu and Aryan Kulkarni

CC: Matthias Felleisen

Date: September 27, 2023

The game state should contain the game map, each player's state, the order of
the turns, the current turn, and the list of tiles. The referee is responsible
for maintaining a valid game state and prompting each player upon each turn.
This information would be in a Referee class:

    public class Referee
    {
        private Map _map;
        private List<Player> _players;
        private List<Tile> _tiles;

Our wishlist for the referee's methods include one that returns which player
should move for the current turn:

        public Player CurrentTurn();

A method that receives a move and checks its validity, returning true and
committing it to the game state iff valid:

        public bool ApplyMove(IMove move);

Next, we should expose the check that the game is over in the API:

        public bool IsGameEnd();

If invalid moves are submitted, the referee may eliminate a player, in which
case it will notify a callback:

        public Action<Player> EliminatePlayerCallback { set; }
    }

Next, we should have a Player class. Each player has a unique ID, a list of
tiles and a score.

    public class Player
    {
        public int Id { get; }
        public List<Tile> Tiles { get; set; }
        public int Score { get; set; }
    }

Finally, we need an IMove interface that is extended by different types of
moves:

    public interface IMove
    {
        public bool IsValidFor(map gameMap);
    }
    public class Pass implements IMove
    public class Exchange implements IMove
    public class Place implements IMove
