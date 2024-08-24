using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicSend
{
    internal static class JsonReadWrite
    {
        public static T? ReadObject<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }

            string objectString = string.Empty;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    objectString = reader.ReadToEnd();
                }
            }

            if (objectString == string.Empty)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(objectString);
        }

        public static void WriteObject<T>(string filePath, T value)
        {
            string objectString = JsonSerializer.Serialize<T>(value);

            if (!File.Exists(filePath))
            {
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(objectString);
            }
        }

    }
}
