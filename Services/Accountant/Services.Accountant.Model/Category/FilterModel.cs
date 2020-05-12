
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountant.Model.Category
{
    [JsonConverter(typeof(ClauseConverter))]
    public abstract class Clause
    {
    }

    public class SingleClause : Clause
    {
        public int Key { get; set; }
        public EnumOperator Operator { get; set; }
        public string[] Values { get; set; }
    }

    public class FilterClause : Clause
    {
        public Clause LeftClause { get; set; }
        public EnumLogicOperator? LogicOperator { get; set; }
        public Clause RightClause { get; set; }
    }


    public class ClauseConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);

        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            return ConvertToClause(token);
             
        }

        private Clause ConvertToClause(JToken token)
        {
            Clause clause = null;

            var props = (token as JObject).Properties();

            bool isSingle = props.Any(c => c.Name == nameof(SingleClause.Operator));

            if (isSingle)
            {
                var key = props.First(c => c.Name.ToString() == nameof(SingleClause.Key)).Value;
                var ope = props.First(c => c.Name.ToString() == nameof(SingleClause.Operator)).Value;
                var values = props.First(c => c.Name.ToString() == nameof(SingleClause.Values)).Value;
                clause = new SingleClause
                {
                    Key = int.Parse(key.ToString()),
                    Operator = (EnumOperator)int.Parse(ope.ToString()),
                    Values = values.Values<string>().ToArray()
                };
            }
            else
            {
                var leftClause = props.FirstOrDefault(c => c.Name.ToString() == nameof(FilterClause.LeftClause))?.Value ?? null;
                var rightClause = props.FirstOrDefault(c => c.Name.ToString() == nameof(FilterClause.RightClause))?.Value ?? null;
                var logicOperator = props.FirstOrDefault(c => c.Name.ToString() == nameof(FilterClause.LogicOperator))?.Value ?? null;
                clause = new FilterClause
                {
                    LeftClause = leftClause != null ? ConvertToClause(leftClause) : null,
                    RightClause = rightClause != null ? ConvertToClause(rightClause) : null,
                    LogicOperator = logicOperator != null ? (EnumLogicOperator)int.Parse(logicOperator.ToString()) : default
                };
            }

            return clause;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Clause) == objectType;
        }

    }


}
