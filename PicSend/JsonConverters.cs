using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicSend
{
    internal class JsonByteArrayConverter : JsonConverter<byte[]>
    {

        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<byte> readBytes = new List<byte>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                readBytes.Add(reader.GetByte());
            }

            return readBytes.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
