# Memorandum

To: CEOs

From: Aryan Kulkarni and Josiah Borgstede

CC: Matthias Felleisen, Ben Lerner

Date: November 9, 2023  
## Classes
```
class ProxyPlayer extends IPlayer
    // On construction, takes a TCP connection and name
    // Will throw if ack not received for setup, takeTurn, newTiles, or win

    public: String name()
    // returns the name of the player
    // this is not a network call

    public: Any setup(Map m, Set<Tile>Bag<Tile> st)
    // ["setup", JMap, [JTile, ..., JTile]]
    // sends a JPlayerCall message on the TCP connection specified on construction

    public: PASS or REPLACE or EXTENSION takeTurn(PublicState s)
    // ["take-turn", JPub]
    // sends a JPlayerCall message on the TCP connection specified on construction
    // throws if the reply is not a JAction
    // throws if it does not receiv a JAction in a certain timeout after receiving an ack

    public Any newTiles(Set<Tile>Bag<Tile> st)
    // ["new-tiles", [JTile, ..., JTile]]
    // sends a JPlayerCall message on the TCP connection specified on construction

    public: Any win(Boolean w)
    // ["win", true] or ["win", false]
    // sends a JPlayerCall message on the TCP connection specified on construction
```

```
class ProxyReferee
    // On construction, takes a player and TCP connection

    public: void run()
    // starts an infinite loop of listening on the specified TCP connection
    // whenever it receives a JPlayerCall, it deserializes it to the proper player method
    //      1. send an ack back on the TCP connection
    //      2. calls the method on the specified player after deserializing the JPlayerCall
    //      3. if the method returns a value, serialize it and send it on the TCP connection

    public: void stop()
    // stops the infinite loop of listening
```
## JSON Definitions
A `JPlayerCall` is one of:  
- `["setup", JMap, [JTile, ..., JTile]]`  
Interpretation: Denotes a call to the Player setup method.  
- `["take-turn", JPub]`  
Interpretation: Denotes a call to the Player takeTurn method.  
- `["new-tiles", [JTile, ..., JTile]]`  
Interpretation: Denotes a call to the Player newTiles method.  
- `["win", true]`
- `["win", false]`  
Interpretation: Denotes a call to the Player win method.  

A `JRegister` is:
- `[Hostname, Port#, JName]`  
Interpretation: All the information needed to setup a proxy player with the given
name that will communicate on the specified IP/Hostname at the given port via TCP  

## Logical Interactions
```
                 Registration Step

Server                                       Client
  |           JRegister                        |
  |<~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ |  % Client requests to join a game
  |                                            |  % message is send via a pre-specified
  |--+                                         |  % IP and port via UDP
  .  |                                         |
  .  | Sets up proxy player                    |  % Setups a proxy player for the
  .  |                                         |  % client that just registered
  .<-+                                         |
  |                                            |

Repeat the above until 4 players or the game should start.
  |  Passes proxy players                      |
  | ---------------------> Referee             | % starts a new game with the
  |                           |                | % created proxy players
  |                           |                |
  |                           |                |
  |       Results             |                |
  | <======================== |                |
  |--+                                         |
  .  |                                         | % Broadcasts the results on a
  .  | Broadcasts results                      | % pre-specified IP
  .  | Award Winners                           |
  .  | Punish misbehaviour                     |
  .<-+                                         |
```
The server is a method that when started will listen on a given port via UDP for
`JRegister` messages. From there `JRegister` messages it will construct a
ProxyPlayer and appends it to a list of `ProxyPlayer`s.  

The client in this diagram is anyone who wants to join a game. It is their
responsibility to ensure that they are able to connect via TCP on the connection
they specified and that they are a able to properly process JPlayerCalls and 
respond accordingly.  

Below is the diagram of how these interaction will unfold if the client decides
to use a proxy referee.
```
                    Interaction between Referee and Player

Referee       Proxy Player            Proxy Referee     IPlayer
  |  Method call  |                         |              |
  | ------------->|       JPlayerCall       |              | % a method in the player
  |               | ~~~~~~~~~~~~~~~~~~~~~~> |              | % API is called by the referee
  |               | <~~~~~~~~~~~~~~~~~~~~~~ |              |
  |               |           Ack           | Method call  | % the Proxy referee deserializes
  |               |                         | -----------> | % the JPlayerCall and calls
  |               |                         |              | % the appropriate method

  If : method has a return
  |               |                         |    Return    |
  |               |         JAction         | <=========== |
  |     Return    | <~~~~~~~~~~~~~~~~~~~~~~ |              |
  | <============ |                         |              |

  Case that player fails to respond
  |  Method call  |                         |              |
  | ------------->|       JPlayerCall       |              | % a method in the player API
  |               | ~~~~~~~~~~~~~~~~~~~~~~> |              |
  |               |--+                      |              |
  |               .--|                      |              |
  |               .--| Waiting for timeout  |              |
  |               .--|                      |              |
  |     Throws    .<-+                      |              |
  | <============ |                         |              |
```

Legend
```
    % on the right are interpretive comments
    ------> are calls
    ~~~~~~> are network communications
    <====== are returns
```

