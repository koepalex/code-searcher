using CodeSearcher.BusinessLogic.SearchResults;
using CodeSearcher.Interfaces;
using Newtonsoft.Json;
using System;

namespace CodeSearcher.BusinessLogic.Serialization
{
    class DetailedResultJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IDetailedSearchResult));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(DetailedSearchResult));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(DetailedSearchResult));
        }
    }
}
