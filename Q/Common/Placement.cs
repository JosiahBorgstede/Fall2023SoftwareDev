using Newtonsoft.Json;

namespace Q.Common;

public record Placement([property: JsonProperty("coordinate")]
                        Coordinate Coordinate,
                        [property: JsonProperty("1tile")]
                        Tile Tile);
