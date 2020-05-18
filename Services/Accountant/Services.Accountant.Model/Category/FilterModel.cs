
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

    public class SameLeveClause
    {
        public Clause Clause { get; set; }
        public EnumLogicOperator? LogicOperator { get; set; }
    }

    public class ArrayClause : Clause
    {
        public SameLeveClause[] Clauses { get; set; }
    }

    public class DoubleClause : Clause
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
            Clause resultClause = null;
            if(!(token is JObject))
            {
                return null;
            }
            var props = (token as JObject).Properties();
            bool isSingle = props.Any(c => c.Name.ToLower() == nameof(SingleClause.Operator).ToLower());
            bool isDouble = props.Any(c => c.Name.ToLower() == nameof(DoubleClause.LogicOperator).ToLower());
            if (isSingle)
            {
                var key = props.First(c => c.Name.ToLower() == nameof(SingleClause.Key).ToLower()).Value;
                var ope = props.First(c => c.Name.ToLower() == nameof(SingleClause.Operator).ToLower()).Value;
                var values = props.First(c => c.Name.ToLower() == nameof(SingleClause.Values).ToLower()).Value;
                resultClause = new SingleClause
                {
                    Key = int.Parse(key.ToString()),
                    Operator = (EnumOperator)int.Parse(ope.ToString()),
                    Values = values.Values<string>().ToArray()
                };
            }
            else if (isDouble)
            {
                var leftClause = props.FirstOrDefault(c => c.Name.ToLower() == nameof(DoubleClause.LeftClause).ToLower())?.Value ?? null;
                var rightClause = props.FirstOrDefault(c => c.Name.ToLower() == nameof(DoubleClause.RightClause).ToLower())?.Value ?? null;
                var logicOperator = props.FirstOrDefault(c => c.Name.ToLower() == nameof(DoubleClause.LogicOperator).ToLower())?.Value ?? null;
                resultClause = new DoubleClause
                {
                    LeftClause = leftClause != null ? ConvertToClause(leftClause) : null,
                    RightClause = rightClause != null ? ConvertToClause(rightClause) : null,
                    LogicOperator = logicOperator != null ? (EnumLogicOperator)int.Parse(logicOperator.ToString()) : default
                };
            }
            else
            {
                var clauses = props.FirstOrDefault(c => c.Name == nameof(ArrayClause.Clauses).ToLower())?.Value ?? null;
                if (clauses != null)
                {
                    var arrClause = clauses.ToArray();
                    List<SameLeveClause> lstClause = new List<SameLeveClause>();
                    foreach (var item in arrClause)
                    {
                        if (!(item is JObject))
                        {
                            continue;
                        }
                        var itemProps = (item as JObject).Properties();
                        var clause = itemProps.FirstOrDefault(c => c.Name.ToLower() == nameof(SameLeveClause.Clause).ToLower())?.Value ?? null;
                        var logicOperator = itemProps.FirstOrDefault(c => c.Name.ToLower() == nameof(SameLeveClause.LogicOperator).ToLower())?.Value ?? null;

                        SameLeveClause sameLeveClause = new SameLeveClause
                        {
                            Clause = clause != null ? ConvertToClause(clause) : null,
                            LogicOperator = logicOperator != null ? (EnumLogicOperator)int.Parse(logicOperator.ToString()) : default
                        };
                        lstClause.Add(sameLeveClause);
                    }
                    resultClause = new ArrayClause
                    {
                        Clauses = lstClause.ToArray()
                    };
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
