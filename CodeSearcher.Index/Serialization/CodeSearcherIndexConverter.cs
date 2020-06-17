using CodeSearcher.BusinessLogic.Management;
using CodeSearcher.Interfaces;
using Newtonsoft.Json;
using System;

namespace CodeSearcher.BusinessLogic.Serialization
{
    class CodeSearcherIndexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ICodeSearcherIndex));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(CodeSearcherIndex));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(CodeSearcherIndex));
        }
    }
}
