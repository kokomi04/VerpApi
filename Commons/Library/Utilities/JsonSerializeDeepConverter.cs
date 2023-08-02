using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.Library.Utilities
{
    public class JsonSerializeDeepConverter : JsonConverter
    {
        private readonly int _depth;

        public JsonSerializeDeepConverter(int depth)
        {
            _depth = depth;
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObject = JsonUtils.GetJobjectNoneLoopDeep(value, new Stack<object>(), 0, _depth);

            //var jObject = JToken.FromObject(value, serializer);
            jObject.WriteTo(writer);
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JsonUtils.JsonSerializer.Deserialize(reader, objectType);
        }
    }

}
