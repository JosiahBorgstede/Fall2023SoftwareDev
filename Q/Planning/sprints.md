# Memorandum

To: CEOs

From: Alan Hu and Aryan Kulkarni

CC: Matthias Felleisen

Date: September 14, 2023

Subject: Sprint Planning

Sprint 1: Design a game referee class which implements the game rules and
manages the game state. The public methods of this class would correspond to
the information that players and observers may see or actions that players
may take to update the game state.

Sprint 2: Create player class that contains "AI" logic. Implement the player
in terms of the API of the game referee class from sprint 1.

Sprint 3: Design a network JSON API that interfaces between referee and players
as well as passive observers. The JSON API would essentially be an RPC to
transmit method calls over the net, as a facade over the referee and player
classes from sprints 1 and 2. Design some validation/authentication system to
validate communications over the net from untrusted actors (e.g. observers
should not be able to modify the game state).
