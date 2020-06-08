
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Infrastructure.EF.EFExtensions
{
    [JsonConverter(typeof(ClauseConverter))]
    public abstract class Clause
    {
    }

    public class SingleClause : Clause
    {
        public int Field { get; set; }
        public string FieldName { get; set; }
        public EnumOperator Operator { get; set; }
        public object Value { get; set; }
    }

    public class ArrayClause : Clause
    {
        public ArrayClause()
        {
            Rules = new List<Clause>();
        }

        public ICollection<Clause> Rules { get; set; }
        public EnumLogicOperator Condition { get; set; }
        public bool Not { get; set; }
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
            Clause resultClause = null;
            if (!(token is JObject))
            {
                return null;
            }
            var props = (token as JObject).Properties();
            bool isSingle = props.Any(c => c.Name.ToLower() == nameof(SingleClause.Operator).ToLower());
            bool isArray = props.Any(c => c.Name.ToLower() == nameof(ArrayClause.Condition).ToLower());
            if (isSingle)
            {
                var key = props.First(c => c.Name.ToLower() == nameof(SingleClause.Field).ToLower()).Value.ToString();
                var fieldName = props.FirstOrDefault(c => c.Name.ToLower() == nameof(SingleClause.FieldName).ToLower())?.Value?.ToString();
                var ope = props.First(c => c.Name.ToLower() == nameof(SingleClause.Operator).ToLower()).Value.ToString();
                var value = props.First(c => c.Name.ToLower() == nameof(SingleClause.Value).ToLower()).Value;
                resultClause = new SingleClause
                {
                    Field = int.Parse(key),
                    FieldName = fieldName,
                    Operator = (EnumOperator)int.Parse(ope),
                    Value = value.ToObject<object>()
                };
            }
            else if(isArray)
            {
                var clauses = props.FirstOrDefault(c => c.Name == nameof(ArrayClause.Rules).ToLower()).Value;
                var logicOperator = props.FirstOrDefault(c => c.Name.ToLower() == nameof(ArrayClause.Condition).ToLower()).Value.ToString();
                var not = props.FirstOrDefault(c => c.Name == nameof(ArrayClause.Not).ToLower()).Value.ToString();

                resultClause = new ArrayClause
                {
                    Condition = AccountantEnumExtensions.GetValueFromDescription<EnumLogicOperator>(logicOperator),
                    Not = bool.Parse(not)
                };
                var arrClause = clauses.ToArray();
                foreach (var item in arrClause)
                {
                    var clause = ConvertToClause(item);
                    (resultClause as ArrayClause).Rules.Add(clause);
                }
            }
            return resultClause;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Clause) == objectType;
        }
    }
}
