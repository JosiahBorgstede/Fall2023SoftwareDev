using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Q.Common;

public record SpectatorInfo
{
    public Map Map;

    /// <summary>
    /// The number of Tiles that the referee still has
    /// </summary>
    public int RemainingTiles;

    /// <summary>
    /// The public information about all the other players, in turn order
    /// </summary>
    public IEnumerable<PublicPlayerInfo> PlayerInfos;

    public SpectatorInfo(Map map,
                         int remainingTiles,
                         IEnumerable<PublicPlayerInfo> playerInfos)
    {
        Map = map;
        RemainingTiles = remainingTiles;
        PlayerInfos = playerInfos;
    }
}

/// <summary>
/// A PlayerTurnInfo exposes the information about the game state that a player
/// will see when it is their turn.
/// </summary>
[JsonConverter(typeof(PlayerTurnInfoConverter))]
public record PlayerTurnInfo : SpectatorInfo
{
    /// <summary>
    /// All the information about a player as it relates to the game state.
    /// </summary>
    public PlayerState CurrentPlayer;

    public PlayerTurnInfo(Map map,
                          PlayerState currentPlayer,
                          IEnumerable<PublicPlayerInfo> otherPlayers,
                          int remainingTiles) : base(map, remainingTiles, otherPlayers)
    {
        CurrentPlayer = currentPlayer;
    }
}

/// A PublicPlayerInfo exposes the information about the player state that all
/// players can see.
public record PublicPlayerInfo(int Score);

public class PlayerTurnInfoConverter : JsonConverter<PlayerTurnInfo>
{
    public override void WriteJson(JsonWriter writer,
                                   PlayerTurnInfo value,
                                   JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("map");
        serializer.Serialize(writer, value.Map);
        writer.WritePropertyName("tile*");
        writer.WriteValue(value.RemainingTiles);
        writer.WritePropertyName("players");
        writer.WriteStartArray();
        serializer.Serialize(writer, value.CurrentPlayer);
        foreach(var player in value.PlayerInfos)
        {
            writer.WriteValue(player.Score);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
    public override bool CanWrite
    {
        get { return true; }
    }
    public override PlayerTurnInfo ReadJson(JsonReader reader,
                                            Type objectType,
                                            PlayerTurnInfo existingValue,
                                            bool hasExistingValue,
                                            JsonSerializer serializer)
    {
        JObject state = JObject.Load(reader);
        return new PlayerTurnInfo(state.GetValue("map")!.ToObject<Map>()!,
                                  GetCurrentPlayer((JArray)state.GetValue("players")!),
                                  ReadPlayers((JArray)state.GetValue("players")!),
                                  state.GetValue("tile*")!.ToObject<int>());
    }
    private PlayerState GetCurrentPlayer(JArray playersArray)
    {
        return playersArray[0].ToObject<PlayerState>()!;
    }
    private IEnumerable<PublicPlayerInfo> ReadPlayers(JArray playersArray)
    {
        List<PublicPlayerInfo> players = new List<PublicPlayerInfo>();
        for(int i = 1; i < playersArray.Count; i++)
        {
            players.Add(new PublicPlayerInfo(playersArray[i].ToObject<int>()));
        }
        return players;
    }
}
