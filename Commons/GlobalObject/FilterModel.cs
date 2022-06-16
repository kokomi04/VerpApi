﻿
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.GlobalObject
{
    [JsonConverter(typeof(ClauseConverter))]
    public abstract class Clause
    {

    }

    public class SingleClause : Clause
    {
        public string FieldName { get; set; }
        public EnumOperator Operator { get; set; }
        public object Value { get; set; }
        public EnumDataType DataType { get; set; }
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
            try
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
                    var fieldName = props.FirstOrDefault(c => c.Name.ToLower() == nameof(SingleClause.FieldName).ToLower())?.Value?.ToString();
                    var ope = props.First(c => c.Name.ToLower() == nameof(SingleClause.Operator).ToLower()).Value.ToString();
                    var value = props.First(c => c.Name.ToLower() == nameof(SingleClause.Value).ToLower()).Value;
                    var dataType = props.First(c => c.Name.ToLower() == nameof(SingleClause.DataType).ToLower()).Value.ToString();
                    resultClause = new SingleClause
                    {
                        DataType = (EnumDataType)int.Parse(dataType),
                        FieldName = fieldName,
                        Operator = (EnumOperator)int.Parse(ope),
                        Value = value.ToObject<object>()
                    };
                }
                else if (isArray)
                {
                    var clauses = props.FirstOrDefault(c => c.Name == nameof(ArrayClause.Rules).ToLower()).Value;
                    var logicOperator = props.FirstOrDefault(c => c.Name.ToLower() == nameof(ArrayClause.Condition).ToLower()).Value.ToString();
                    var not = props.FirstOrDefault(c => c.Name == nameof(ArrayClause.Not).ToLower()).Value.ToString();

                    resultClause = new ArrayClause
                    {
                        Condition = Enums.MasterEnum.EnumValueExtensions.GetValueFromDescription<EnumLogicOperator>(logicOperator),
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
            catch (Exception)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Định dạng bộ lọc truyền lên không hợp lệ");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Clause) == objectType;
        }
    }
}
