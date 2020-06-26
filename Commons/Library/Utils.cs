using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library
{
    public static class Utils
    {
        public static Guid ToGuid(this string value)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Unicode.GetBytes(value.ToLower()));
            return new Guid(data);
        }

        public static Guid HashApiEndpointId(int serviceId, string route, EnumMethod method)
        {
            var service = "";
            if (serviceId > 0)
            {
                service = serviceId.ToString();
            }

            route = (route ?? "").Trim().ToLower();
            return $"{service}{route}{method}".ToGuid();
        }

        public static EnumAction GetDefaultAction(this EnumMethod method)
        {
            switch (method)
            {
                case EnumMethod.Get:
                    return EnumAction.View;
                case EnumMethod.Post:
                    return EnumAction.Add;
                case EnumMethod.Put:
                    return EnumAction.Update;
                case EnumMethod.Delete:
                    return EnumAction.Delete;
            }

            return EnumAction.View;
        }

        public static string GetObjectJsonDiff(string existing, object modified)
        {
            if (existing == null)
            {
                return modified.JsonSerialize();
            }
            if (modified == null)
            {
                return existing;
            }

            JObject xptJson = JObject.FromObject(modified, JsonSerializer.Create(settings));
            JObject actualJson = JObject.Parse(existing);

            var xptProps = xptJson.Properties().ToList();
            var actProps = actualJson.Properties().ToList();

            var changes = (from existingProp in actProps
                           from modifiedProp in xptProps
                           where modifiedProp.Path.Equals(existingProp.Path)
                           where !modifiedProp.Value.Equals(existingProp.Value)
                           select new
                           {
                               Field = existingProp.Path,
                               OldValue = existingProp.Value.ToString(),
                               NewValue = modifiedProp.Value.ToString(),
                           }).ToList();

            var obj = JObject.Parse("{}");
            foreach (var item in changes)
            {
                obj[item.Field] = item.NewValue;
            }

            return obj.JsonSerialize();
        }

        public static string GetJsonDiff(string existing, object modified)
        {
            if (existing == null)
            {
                return modified.JsonSerialize();
            }
            if (modified == null)
            {
                return existing;
            }
            JToken mod = JToken.FromObject(modified, JsonSerializer.Create(settings));
            JToken org = JToken.Parse(existing);

            if (mod is JObject && org is JObject)
            {
                return GetObjectJsonDiff(existing, modified);
            }
            return mod.JsonSerialize();
        }

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };
        public static string JsonSerialize(this object obj)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public static T JsonDeserialize<T>(this string obj)
        {
            if (string.IsNullOrWhiteSpace(obj)) return default(T);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj);
        }

        public static long GetUnix(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long? GetUnix(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return (long)dateTime.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime? UnixToDateTime(this long unixTime)
        {
            // if (unixTime == 0) return null;
            return new DateTime(1970, 1, 1).AddSeconds(unixTime);
        }

        public static decimal Eval(string expression)
        {
            try
            {
                var outPut = new NCalc.Expression(expression).Evaluate();
                var result = Convert.ToDecimal(outPut);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static decimal EvalPrimaryQuantityFromProductUnitConversionQuantity(decimal productUnitConversionQuantity, string factorExpression)
        {
            var expression = $"({productUnitConversionQuantity})/({factorExpression})";
            return Eval(expression);
        }

        public static (bool, decimal) GetPrimaryQuantityFromProductUnitConversionQuantity(decimal productUnitConversionQuantity, string factorExpression, decimal inputData)
        {
            var expression = $"({productUnitConversionQuantity})/({factorExpression})";
            var value = Eval(expression);
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData);
            }

            if (inputData == 0)
            {
                return (true, value);
            }
            else
            {
                return (false, value);
            }
        }

        public static (bool, decimal) GetPrimaryQuantityFromProductUnitConversionQuantity(decimal productUnitConversionQuantity, decimal factorExpression, decimal inputData)
        {
            var value = productUnitConversionQuantity / factorExpression;
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData);
            }

            if (inputData == 0)
            {
                return (true, value);
            }
            else
            {
                return (false, value);
            }
        }

        public static (bool, decimal) GetProductUnitConversionQuantityFromPrimaryQuantity(decimal primaryQuantity, string factorExpression, decimal inputData)
        {
            var expression = $"({primaryQuantity})*({factorExpression})";
            var value = Eval(expression);
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData);
            }

            if (inputData == 0)
            {
                return (true, value);
            }
            else
            {
                return (false, value);
            }

        }

        public static (bool, decimal) GetProductUnitConversionQuantityFromPrimaryQuantity(decimal primaryQuantity, decimal factorExpression, decimal inputData)
        {
            var value = primaryQuantity * factorExpression;
            if (Math.Abs(value - inputData) <= Numbers.INPUT_RATE_STANDARD_ERROR)
            {
                return (true, inputData);
            }

            if (inputData == 0)
            {
                return (true, value);
            }
            else
            {
                return (false, value);
            }

        }

        public static string GetObjectKey(EnumObjectType objectTypeId, long objectId)
        {
            return $"{((int)objectTypeId)}_{objectId}";
        }

        public static Expression<Func<T, object>> ToMemberOf<T>(this string name)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyOrField = Expression.PropertyOrField(parameter, name);
            var unaryExpression = Expression.MakeUnary(ExpressionType.Convert, propertyOrField, typeof(object));

            return Expression.Lambda<Func<T, object>>(unaryExpression, parameter);
        }

        public static IQueryable<T> SortByFieldName<T>(this IQueryable<T> query, string filedName, bool asc)
        {
            var type = typeof(T);

            PropertyInfo propertyInfo = null;
            foreach (var p in type.GetProperties())
            {
                if (p.Name.Equals(filedName, StringComparison.OrdinalIgnoreCase))
                {
                    propertyInfo = p;
                    break;
                }
            }
            if (propertyInfo == null) return query;

            var ex = propertyInfo.Name.ToMemberOf<T>();
            return asc ? query.OrderBy(ex) : query.OrderByDescending(ex);
        }
        public static string Format(this decimal number, int decimalplace = 16)
        {
            var format = new StringBuilder();
            format.Append("#,#.");
            for (var i = 1; i < decimalplace; i++)
            {
                format.Append("#");
            }
            return number.ToString(format.ToString());
        }

        public static decimal AddDecimal(this decimal a, decimal b)
        {
            if (a < 0 && b > 0 || a > 0 && b < 0)
            {
                var c = a + b;
                if (Math.Abs(c) < Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
                return c;
            }
            return a + b;
        }

        public static decimal SubDecimal(this decimal a, decimal b)
        {
            if (a > 0 && b > 0 || a < 0 && b < 0)
            {
                var c = a - b;
                if (Math.Abs(c) < Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
                return c;
            }
            return a - b;
        }

        public static decimal RelativeTo(this decimal value, decimal relValue)
        {
            if (Math.Abs(value) < Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
            if (value.SubDecimal(relValue) == 0) return relValue;
            return value;
        }

        public static long ConvertValueToNumber(this string value, EnumDataType dataType)
        {
            long valueInNumber;
            switch (dataType)
            {
                case EnumDataType.Boolean:
                    valueInNumber = bool.Parse(value) ? 1 : 0;
                    break;
                case EnumDataType.Int:
                case EnumDataType.Percentage:
                    valueInNumber = (long)(double.Parse(value) * AccountantConstants.CONVERT_VALUE_TO_NUMBER_FACTOR);
                    break;
                case EnumDataType.Date:
                    valueInNumber = (long)(double.Parse(value));
                    break;
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    valueInNumber = value.GetLongHash();
                    break;
            }

            return valueInNumber;
        }

        public static long GetLongHash(this string input)
        {
            long hashCode = 0;
            if (!string.IsNullOrEmpty(input))
            {
                //Unicode Encode Covering all characterset
                byte[] byteContents = Encoding.Unicode.GetBytes(input);
                SHA256 hash = new SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                //32Byte hashText separate
                //hashCodeStart = 0~7  8Byte
                //hashCodeMedium = 8~23  8Byte
                //hashCodeEnd = 24~31  8Byte
                //and Fold
                Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
                Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
                Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }
            return (hashCode);
        }

        public static string ConvertValueToData(this string value, EnumDataType dataType)
        {
            switch (dataType)
            {
                case EnumDataType.Boolean:
                    value = value.ToUpper();

                    break;
                case EnumDataType.Date:
                    long valueInNumber = long.Parse(value);
                    value = valueInNumber.UnixToDateTime()?.ToString(DateFormats.DD_MM_YYYY);
                    break;
                case EnumDataType.Percentage:
                case EnumDataType.Int:
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    break;
            }

            return value;
        }




        public static Type GetColumnDataType(this EnumDataType dataType)
        {
            switch (dataType)
            {
                case EnumDataType.Text:
                    return typeof(string);
                case EnumDataType.Int: return typeof(int);
                case EnumDataType.Date: return typeof(DateTime);
                case EnumDataType.PhoneNumber: return typeof(string);
                case EnumDataType.Email: return typeof(string);
                case EnumDataType.Boolean: return typeof(bool);
                case EnumDataType.Percentage: return typeof(short);
                case EnumDataType.BigInt: return typeof(long);
                case EnumDataType.Decimal: return typeof(decimal);
                default: return typeof(string);
            }
        }

        public static object GetSqlValue(this EnumDataType dataType, object value)
        {
            if (value == null) return DBNull.Value;

            if (value.GetType() == typeof(string))
            {
                value = (value as string).Trim();

                if (string.Empty.Equals(value)) return DBNull.Value;
            }

            switch (dataType)
            {
                case EnumDataType.Text:
                    return value?.ToString();
                case EnumDataType.Int:
                    int intValue;
                    if (!int.TryParse(value.ToString(), out intValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu int");
                    }
                    return intValue;

                case EnumDataType.Date:
                    long dateValue;
                    if (!long.TryParse(value.ToString(), out dateValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu ngày tháng");
                    }
                    return dateValue.UnixToDateTime();
                case EnumDataType.PhoneNumber: return value?.ToString();
                case EnumDataType.Email: return value?.ToString();
                case EnumDataType.Boolean:
                    bool boolValue;
                    if (!bool.TryParse(value.ToString(), out boolValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu logic");
                    }
                    return boolValue;
                case EnumDataType.Percentage:
                    short percentValue;
                    if (!short.TryParse(value.ToString(), out percentValue)|| percentValue < -100 || percentValue > 100)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu phần trăm");
                    }
                    return percentValue;
                case EnumDataType.BigInt:
                    long longValue;
                    if (!long.TryParse(value.ToString(), out longValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu long");
                    }
                    return longValue;
                case EnumDataType.Decimal:
                    decimal decimalValue;
                    if (!decimal.TryParse(value.ToString(), out decimalValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu decimal");
                    }
                    return decimalValue;
                default: return value?.ToString();
            }
        }

        public static object ConvertValueByType(this string value, EnumDataType dataType)
        {
            object result;
            switch (dataType)
            {
                case EnumDataType.Boolean:
                    result = value.Trim().ToLower() == true.ToString().ToLower() || value.Trim() == "1";
                    break;
                case EnumDataType.Date:
                    result = DateTime.ParseExact(value, DateFormats.DD_MM_YYYY, CultureInfo.InvariantCulture);
                    break;
                case EnumDataType.Percentage:
                case EnumDataType.Decimal:
                    result = double.Parse(value);
                    break;
                case EnumDataType.Int:
                    result = int.Parse(value);
                    break;
                case EnumDataType.BigInt:
                    result = long.Parse(value);
                    break;
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    result = value;
                    break;
            }

            return result;
        }

    }
}
