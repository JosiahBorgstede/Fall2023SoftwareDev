using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Q.Common.RefereeConfiguration;

namespace Q.Common;

/// Class for parameterizable aspects of the game.
public class Settings
{
    public int QBonus { get; set; }
    public int WinBonus { get; set; }

    public TimeSpan ResponseTime { get; set; }

    public static Settings QSettings = new Settings(8, 4, TimeSpan.FromSeconds(6));

    /// Constructs the game settings.
    /// qBonus: Number of points gained for scoring a Q.
    /// winBonus: Number of points for placing all tiles.
    public Settings(int qBonus, int winBonus)
    {
        QBonus = qBonus;
        WinBonus = winBonus;
    }

    /// Constructs the game settings.
    /// qBonus: Number of points gained for scoring a Q.
    /// winBonus: Number of points for placing all tiles.
    /// responseTIme: the amount of time a player has to responde before kick
    public Settings(int qBonus, int winBonus, TimeSpan responseTime)
    {
        QBonus = qBonus;
        WinBonus = winBonus;
        ResponseTime = responseTime;
    }
}

public record ScoreConfiguration([property: JsonProperty("qbo")] int QBonus,
                                 [property: JsonProperty("fbo")] int WinBonus);

[JsonConverter(typeof(RefereeConfigurationConverter))]
public record RefereeConfiguration
{
    public ScoreConfiguration scoreConfiguration;

    public IGameState? state;

    public TimeSpan timePerPlayerAction;

    public bool DebugEnabled;

    public bool ObserverEnabled;

    private RefereeConfiguration(ScoreConfiguration scoreConfiguration,
                                 IGameState? state,
                                 TimeSpan perTurn,
                                 bool debug,
                                 bool observer)
    {
        this.scoreConfiguration = scoreConfiguration;
        this.state = state;
        this.timePerPlayerAction = perTurn;
        this.DebugEnabled = debug;
        ObserverEnabled = observer;
    }

    public class RefereeConfigBuilder
    {
        private ScoreConfiguration scoreConfig = new ScoreConfiguration(8, 4);

        private TimeSpan perTurn;

        private IGameState? startState;

        private bool DebugEnabled = false;

        private bool ObserverEnabled = false;

        public RefereeConfigBuilder()
        {
            perTurn = TimeSpan.Zero;

        }

        public RefereeConfigBuilder TimePerTurnSeconds(int seconds)
        {
            perTurn += TimeSpan.FromSeconds(seconds);
            return this;
        }

        public RefereeConfigBuilder EnableDebug()
        {
            DebugEnabled = true;
            return this;
        }

        public RefereeConfigBuilder EnableObserver()
        {
            ObserverEnabled = true;
            return this;
        }

        public RefereeConfigBuilder AddState(IGameState state)
        {
            startState = state;
            return this;
        }

        public RefereeConfigBuilder SetScoreConfiguration(ScoreConfiguration config)
        {
            scoreConfig = config;
            return this;
        }

        public RefereeConfiguration Build()
        {
            return new RefereeConfiguration(scoreConfig, startState, perTurn, DebugEnabled, ObserverEnabled);
        }
    }

    public class RefereeConfigurationConverter : JsonConverter<RefereeConfiguration>
    {

        public override RefereeConfiguration? ReadJson(JsonReader reader,
                                                       Type objectType,
                                                       RefereeConfiguration? existingValue,
                                                       bool hasExistingValue,
                                                       JsonSerializer serializer)
        {
            JObject config = JObject.Load(reader);
            return new RefereeConfiguration(
                config.GetValue("config-s")!.ToObject<ScoreConfiguration>()!,
                config.GetValue("state0")!.ToObject<GameState>()!,
                TimeSpan.FromSeconds(config.GetValue("per-turn")!.ToObject<int>()),
                !config.GetValue("quiet")!.ToObject<bool>(),
                config.GetValue("observe")!.ToObject<bool>());
        }

        public override void WriteJson(JsonWriter writer,
                                       RefereeConfiguration? value,
                                       JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("config-s");
            serializer.Serialize(writer, value.scoreConfiguration);
            writer.WritePropertyName("quiet");
            writer.WriteValue(!value.DebugEnabled);
            writer.WritePropertyName("observe");
            writer.WriteValue(value.ObserverEnabled);
            writer.WritePropertyName("per-turn");
            writer.WriteValue(value.timePerPlayerAction.Seconds);
            writer.WritePropertyName("state0");
            serializer.Serialize(writer, value.state);
            writer.WriteEndObject();
        }
    }
}

[JsonConverter(typeof(ServerConfigurationConverter))]
public record ServerConfiguration
{
    public int PortNumber;

    public int NumWaitingPeriods;

    public TimeSpan TimePerWaitPeriod;

    public TimeSpan PlayerNameWait;

    public bool DebugEnabled;

    public RefereeConfiguration refereeConfiguration;

    public ServerConfiguration(int portNumber,
                               int numWaitingPeriods,
                               TimeSpan timePerWaitPeriod,
                               TimeSpan playerNameWait,
                               bool debugEnabled,
                               RefereeConfiguration refereeConfiguration)
    {
        this.PortNumber = portNumber;
        this.NumWaitingPeriods = numWaitingPeriods;
        this.TimePerWaitPeriod = timePerWaitPeriod;
        this.refereeConfiguration = refereeConfiguration;
        this.DebugEnabled = debugEnabled;
        this.PlayerNameWait = playerNameWait;
    }

    public class ServerConfigBuilder
    {
        private RefereeConfiguration RefereeConfiguration;

        private int PortNumber;

        private int ServerTries;

        private TimeSpan ServerWaitTime;

        private TimeSpan PlayerNameWaitTime;

        private bool DebugEnabled;

        public ServerConfigBuilder()
        {
            ServerWaitTime = TimeSpan.Zero;
            PlayerNameWaitTime = TimeSpan.Zero;
        }

        public ServerConfigBuilder SetPortNumber(int portNum)
        {
            PortNumber = portNum;
            return this;
        }

        public ServerConfigBuilder SetServerTries(int serverTries)
        {
            ServerTries = serverTries;
            return this;
        }

        public ServerConfigBuilder SetServerWaitTime(int serverWait)
        {
            ServerWaitTime += TimeSpan.FromSeconds(serverWait);
            return this;
        }

        public ServerConfigBuilder SetPlayerNameWaitTime(int signUpWaitTime)
        {
            PlayerNameWaitTime += TimeSpan.FromSeconds(signUpWaitTime);
            return this;
        }

        public ServerConfigBuilder SetRefereeConfiguration(RefereeConfiguration refereeConfiguration)
        {
            RefereeConfiguration = refereeConfiguration;
            return this;
        }

        public ServerConfigBuilder EnableDebug()
        {
            DebugEnabled = true;
            return this;
        }

        public ServerConfiguration Build()
        {
            return new ServerConfiguration(PortNumber,
                                           ServerTries,
                                           ServerWaitTime,
                                           PlayerNameWaitTime,
                                           DebugEnabled,
                                           RefereeConfiguration);
        }

    }

    public class ServerConfigurationConverter : JsonConverter<ServerConfiguration>
    {

        public override ServerConfiguration? ReadJson(JsonReader reader,
                                                       Type objectType,
                                                       ServerConfiguration? existingValue,
                                                       bool hasExistingValue,
                                                       JsonSerializer serializer)
        {
            JObject config = JObject.Load(reader);
            return new ServerConfiguration(
                config.GetValue("port")!.ToObject<int>(),
                config.GetValue("server-tries")!.ToObject<int>(),
                TimeSpan.FromSeconds(config.GetValue("server-wait")!.ToObject<int>()),
                TimeSpan.FromSeconds(config.GetValue("wait-for-signup")!.ToObject<int>()),
                !config.GetValue("quiet")!.ToObject<bool>(),
                config.GetValue("ref-spec")!.ToObject<RefereeConfiguration>()!);
        }

        public override void WriteJson(JsonWriter writer,
                                       ServerConfiguration? value,
                                       JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("port");
            writer.WriteValue(value.PortNumber);
            writer.WritePropertyName("server-tries");
            writer.WriteValue(value.NumWaitingPeriods);
            writer.WritePropertyName("quiet");
            writer.WriteValue(!value.DebugEnabled);
            writer.WritePropertyName("ref-spec");
            serializer.Serialize(writer, value.refereeConfiguration);
            writer.WritePropertyName("server-wait");
            writer.WriteValue(value.TimePerWaitPeriod.Seconds);
            writer.WritePropertyName("wait-for-signup");
            writer.WriteValue(value.PlayerNameWait.Seconds);
            writer.WriteEndObject();
        }
    }

}
