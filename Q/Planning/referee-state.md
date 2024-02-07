# Memorandum

To: CEOs

From: Alan Hu and Josiah Borgstede

CC: Matthias Felleisen

Date: September 14, 2023

Subject: Referee-State

## The Interface

`void KickPlayer(ID)` removes the player with the given ID from the game.  
`bool IsGameOver()` determines if the game is over.  
`void CommitAction(Action)` commits the given action to the game state.  
`PublicGameInfo GetPublicInfo()` gets the public info of the game where public
in this case means information that all players, or spectators, could see.  
`PlayerTurnInfo GetCurrentPlayerTurnInfo()` Gets all the information that the
current player will need in order to take their current turn, i.e. The current
game map and the players tiles.  

## Sequence Diagram

<img align="right" src="Media/Referee-State.png" alt="drawing" height="500"/>

This sequence diagram begins after all of the players have already registered
for the game. The players will not recieve a list of tiles at the start of the
game, instead recieving them when prompting for their first turn of the game
along with all other turn information. (This behavior is consistent with the
message sequence of turns in general, in which players receive their current
list of tiles as part of the player turn information.)

The referee will then retrieve from the game-state the information required for
the current turn. This information will also include some form of player ID so
that the referee can determine which player to send information to without
having to keep track of the turn order. The referee will send the turn
information to the appropriate player and await their response.

Upon receiving an action response, the referee will first check if the action is
valid. (Actions only need the publicly known turn info to be validated.) If the
action is invalid, the referee will kick the player from the game, notifying
both the game-state and the player, then sever the player's connection. If the
action is valid, the referee will commit the action to the game-state.

The referee will then check the game-state to see if the game is over. If it is,
the referee will collect the public information from the game-state and
determine a winner, then notify all the players of the game's end and results.

If the game has not ended, the referee will return to getting all the
information required for the current turn from the game state, and play will
continue as before. This will continue until the game state says that the game
has ended.
