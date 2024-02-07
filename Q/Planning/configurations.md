## Configuration Refactor

In order to add configuration to server, referee, and scoring, first we created

- `ServerConfiguration`
- `RefereeConfiguration`
- `ScoreConfiguration`

Which all live inside of `Q\Common\Settings.cs`

Each of this is a configuration object for their respective components. They are
mainly just `record`s, which makes them immutable data components. For both
`ServerConfiguration` and `RefereeConfiguration` we also made builders for each
of them, making their primary constructors private, so that the builder is the
primary way to use them. We did not do this for `ScoreConfiguration` as it is
only 2 integer values, and not worth creating an entire builder.  
We also made `JsonConverters` for `ServerConfiguration` and
`RefereeConfiguration`, which are internal so that they can use the private
constructors.  

To use the `ServerConfiguration` we only needed to make a few changes to
`Q\Server\server.cs`, namely adding a constructor that takes the configuration
object, and then also refactor a bit to use those values. We also added an
overload for `StartSignup` that takes no parameters. The parameterless one will
use the port specified in the configuration, and the other will use the port
given.

To use the `RefereeConfiguration` we had to make the `RefereeDriver` is no
longer only static functionality, but instead has a constructor that takes a
configuration. The configuration actually uses a nullable `GameState`, and if it
is `null` then the `Referee` will create a new default `GameState` for the list
of players, otherwise it will use the `GameState` passed in in configuration.  

To use `ScoreConfiguration` required no changes as we had already abstracted
over this, the only change was making it be a field of the `RefereeDriver`.