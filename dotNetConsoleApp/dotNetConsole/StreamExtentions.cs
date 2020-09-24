using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotNetConsole
{
    public static class StreamExtentions
    {
        public static List<T> ReadAndDeserializeFromJson<T>(this Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream.");
            }
            JObject newResult = new JObject();
            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerialilzer = new JsonSerializer();
                    newResult = jsonSerialilzer.Deserialize(jsonTextReader) as JObject;
                }
            }
            var clientArray = newResult["value"].Value<JArray>();
            return clientArray.ToObject<List<T>>();
    
        }
    }
}
