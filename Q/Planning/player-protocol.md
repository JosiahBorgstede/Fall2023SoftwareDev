# Memorandum

To: CEOs

From: Alan Hu and Josiah Borgstede

CC: Matthias Felleisen

<img align="right" src="Media/MultiplePlayersSequence.png" alt="drawing" height="600"/>

Each player sends a message to the referee to join the game. The referee sends
back an acknowledgement message immediately after receiving a message to join
the game.  


When the game manager indicates that the game should start, the referee will
send each player a message notifying that the game has begun, containing their
starting tiles.  


For each turn, the referee will send the active player a message containing the
game state visible to it. The player will send back an action message. The
referee will either acknowledge the action if it is valid or send the player
back a kick message if it is invalid. If the player does not respond by a
timeout, the referee will send a kick message.  


At the end of the game, the referee will send each player a message containing
the game results.  


