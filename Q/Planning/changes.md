# Memorandum

To: CEOs

From: Alan Hu and Josiah Borgstede

CC: Matthias Felleisen

Date: November 2, 2023

- changing the bonus points again and allow more players to participate in a
  game;

We rank this a 1. Changing the bonus points is just a matter of changing the
arguments for one method call. Allowing more players to participate in the
game involves changing the logic of the GameState.Builder constructor and the
GameState.Constructor. We would rank this a 2 if we needed to make the
requirements on number of participants parameterized rather than hardcoded.

- adding wildcard tiles;

We rank this a 3. We would need to change both the placement code in Map.cs
and scoring code in IAction.cs. Changing the placement code is simpler, but
changing the code for scoring a Q would require major logic changes.

- imposing restrictions that enforce the rules of Qwirkle instead of Q.

We rank this a 4. The placement code would have to be changed a lot to check
an entire row or column of tiles. We would also have to significantly change
the scoring code. Our code is loosely coupled enough that we could
factor out shared components between Q and Qwirkle, but we would not be able to
reuse our code for scoring the Q game to implement Qwirkle.
