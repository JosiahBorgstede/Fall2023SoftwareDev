using Newtonsoft.Json;
using SkiaSharp;
using Newtonsoft.Json.Linq;
namespace Q.Common;

[JsonConverter(typeof(MapConverter))]
public partial class Map
{
    public class MapConverter : JsonConverter<Map>
    {
        public override void WriteJson(JsonWriter writer,
                                       Map value,
                                       JsonSerializer serializer)
        {
            writer.WriteStartArray();
            var placements = value.Placements();
            int minRow = placements.Min(
                (placement) => {return placement.Coordinate.Y;});
            int maxRow = placements.Max(
                (placement) => {return placement.Coordinate.Y;});
            for(int i = minRow; i <= maxRow; i++)
            {
                IEnumerable<Placement> row = from placement in placements
                                             where placement.Coordinate.Y == i
                                             select placement;
                WriteJRow(writer, row, serializer);
            }
            writer.WriteEndArray();
        }
        private void WriteJRow(JsonWriter writer,
                               IEnumerable<Placement> placements,
                               JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(placements.First().Coordinate.Y);
            foreach(var placement in placements)
            {
                WriteJCell(writer, placement, serializer);
            }
            writer.WriteEndArray();
        }
        private void WriteJCell(JsonWriter writer,
                                Placement value,
                                JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Coordinate.X);
            serializer.Serialize(writer, value.Tile);
            writer.WriteEndArray();
        }
        public override bool CanWrite
        {
            get { return true; }
        }

        public override Map ReadJson(JsonReader reader,
                                     Type objectType,
                                     Map existingValue,
                                     bool hasExistingValue,
                                     JsonSerializer serializer)
        {
            Map map = new Map();
            JArray array = JArray.Load(reader);
            foreach (var rowToken in array)
            {
                var row = (JArray)rowToken;
                int rowIndex = (int)row[0];
                for (var i = 1; i < row.Count; ++i)
                {
                    var cellToken = row[i];
                    var cell = (JArray)cellToken;
                    var colIndex = (int)cell[0];
                    var tileObj = (JObject)cell[1];
                    var tile = tileObj.ToObject<Tile>();
                    map = map.SetTile(new Coordinate(Y: rowIndex, X: colIndex), tile);
                }
            }
            if (reader.TokenType != JsonToken.EndArray)
            {
                throw new JsonReaderException("Expected end array for map");
            }
            return map;
        }
    }
}
