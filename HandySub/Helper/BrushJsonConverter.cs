using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using System.Windows.Media;

namespace HandySub
{
    public class BrushJsonConverter : JsonConverter<Brush>
    {
        public override Brush Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (Brush)XamlReader.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Brush value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(XamlWriter.Save(value));
        }
    }
}
