using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Commons.Library
{
    public static class Utils
    {
        public static string RemoveDiacritics(this string str)
        {
            if (str == null) return null;
            var chars =
                from c in str.Normalize(NormalizationForm.FormD).ToCharArray()
                let uc = CharUnicodeInfo.GetUnicodeCategory(c)
                where uc != UnicodeCategory.NonSpacingMark
                select c;

            var cleanStr = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

            return cleanStr.Replace("đ", "d").Replace("Đ", "D");
        }
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

        public static EnumActionType GetDefaultAction(this EnumMethod method)
        {
            switch (method)
            {
                case EnumMethod.Get:
                    return EnumActionType.View;
                case EnumMethod.Post:
                    return EnumActionType.Add;
                case EnumMethod.Put:
                    return EnumActionType.Update;
                case EnumMethod.Delete:
                    return EnumActionType.Delete;
            }

            return EnumActionType.View;
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

        public static string JsonSerialize(this object obj, bool isIgnoreSensitiveData)
        {
            try
            {
                var cfg = settings;
                if (isIgnoreSensitiveData)
                {
                    cfg.ContractResolver = new SensitiveDataResolver();
                }
                else
                {
                    cfg.ContractResolver = null;
                }

                return JsonConvert.SerializeObject(obj, cfg);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static string JsonSerialize(this object obj)
        {
            return obj.JsonSerialize(false);

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
            return UnixToDateTime((long?)unixTime, null);
        }

        public static DateTime? UnixToDateTime(this long? unixTime)
        {
            return UnixToDateTime(unixTime, null);
        }

        public static DateTime? UnixToDateTime(this long? unixTime, int? timezoneOffset)
        {
            if (unixTime == 0 || !unixTime.HasValue) return null;
            return new DateTime(1970, 1, 1).AddSeconds(unixTime.Value).AddMinutes(-timezoneOffset ?? 0);
        }

        public static DateTime UnixToDateTime(this long unixTime, int? timezoneOffset)
        {
            return UnixToDateTime((long?)unixTime, timezoneOffset).Value;
        }

        public static decimal Eval(string expression)
        {
            try
            {
                var outPut = new NCalc.Expression(expression).Evaluate();
                var result = Convert.ToDecimal(outPut);
                return result;
            }
            catch (Exception)
            {
                throw;
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

        public static decimal SubProductionDecimal(this decimal a, decimal b)
        {
            if (a > 0 && b > 0 || a < 0 && b < 0)
            {
                var c = a - b;
                if (Math.Abs(c) < Numbers.PRODUCTION_MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
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
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
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
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
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

        public static string FormatStyle(string template, string code, long? fId, DateTime? dateTime, string number)
        {
            if (string.IsNullOrWhiteSpace(template)) return template;
            var values = new Dictionary<string, object>{
                { StringTemplateConstants.CODE, code },
                { StringTemplateConstants.FID, fId },
            };

            if (dateTime.HasValue)
            {
                var dateReg = new Regex("\\%DATE\\((?<format>[^\\)]*)\\)\\%");
                foreach (Match m in dateReg.Matches(template))
                {
                    values.Add(m.Value, dateTime.Value.ToString(m.Groups["format"].Value));
                }
            }

            if (!string.IsNullOrWhiteSpace(number))
            {
                values.Add(StringTemplateConstants.SNUMBER, number);
            }
            return FormatStyle(template, values)?.Replace("%", "");
        }

        public static string FormatStyle(string template, IDictionary<string, object> data)
        {
            foreach (var item in data)
            {
                template = template?.Replace(item.Key, item.Value?.ToString());
            }
            return template;
        }

        public static Type GetColumnDataType(this EnumDataType dataType)
        {
            switch (dataType)
            {
                case EnumDataType.Text:
                    return typeof(string);
                case EnumDataType.Int: return typeof(int);

                case EnumDataType.Date:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
                    return typeof(DateTime);

                case EnumDataType.PhoneNumber: return typeof(string);
                case EnumDataType.Email: return typeof(string);
                case EnumDataType.Boolean: return typeof(bool);
                case EnumDataType.Percentage: return typeof(short);
                case EnumDataType.BigInt: return typeof(long);
                case EnumDataType.Decimal: return typeof(decimal);
                default: return typeof(string);
            }
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };
        public static bool IsNumber(this Type objectType)
        {
            return NumericTypes.Contains(objectType);
        }

        public static bool IsNullObject(this object obj)
        {
            if (obj == null || obj == DBNull.Value) return true;

            if (obj.GetType() == typeof(string))
            {
                obj = (obj as string).Trim();

                if (string.Empty.Equals(obj)) return true;
            }

            return false;
        }

        public static bool IsTimeType(this EnumDataType type)
        {
            return AccountantConstants.TIME_TYPES.Contains(type);
        }

        public static bool Convertible(this EnumDataType oldType, EnumDataType newType)
        {
            if (oldType.IsTimeType() && !newType.IsTimeType() && newType != EnumDataType.Text)
            {
                return false;
            }

            return true;
        }

        public static object GetSqlValue(this EnumDataType dataType, object value, int? timeZoneOffset = null)
        {
            if (value.IsNullObject()) return DBNull.Value;

            switch (dataType)
            {
                case EnumDataType.Text:
                    return value?.ToString();
                case EnumDataType.Int:
                    int intValue;
                    try
                    {
                        intValue = Convert.ToInt32(value);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu int");
                    }

                    return intValue;

                case EnumDataType.Date:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
                    long? dateValue;
                    try
                    {
                        dateValue = Convert.ToInt64(value);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu ngày tháng");
                    }

                    if (dateValue == 0) return DBNull.Value;
                    return dateValue.UnixToDateTime(timeZoneOffset).Value;

                case EnumDataType.PhoneNumber: return value?.ToString();
                case EnumDataType.Email: return value?.ToString();
                case EnumDataType.Boolean:
                    bool boolValue;
                    try
                    {
                        boolValue = Convert.ToBoolean(value);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu logic");
                    }
                    return boolValue;
                case EnumDataType.Percentage:
                    float percentValue;
                    if (!float.TryParse(value.ToString(), out percentValue) || percentValue < -100 || percentValue > 100)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu phần trăm");
                    }
                    return percentValue;
                case EnumDataType.BigInt:
                    long longValue;

                    try
                    {
                        longValue = Convert.ToInt64(value);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu long");
                    }
                    return longValue;
                case EnumDataType.Decimal:
                    decimal decimalValue;
                    if (!decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimalValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value} sang kiểu decimal");
                    }
                    return decimalValue;
                default: return value?.ToString();
            }
        }

        public static EnumExcelType GetExcelType(this EnumDataType dataType)
        {
            switch (dataType)
            {
                case EnumDataType.Boolean:
                    return EnumExcelType.Boolean;
                case EnumDataType.Int:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:

                case EnumDataType.Percentage:
                case EnumDataType.BigInt:
                case EnumDataType.Decimal:
                    return EnumExcelType.Number;
                case EnumDataType.Date:
                    return EnumExcelType.DateTime;
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    return EnumExcelType.String;
            }
        }


        public static string GetExcelColumnName(this int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static bool IsSelectForm(this EnumFormType formType)
        {
            return AccountantConstants.SELECT_FORM_TYPES.Contains(formType);
        }

        public static bool IsJoinForm(this EnumFormType formType)
        {
            return AccountantConstants.JOIN_FORM_TYPES.Contains(formType);
        }

        public static int CompareValue(this EnumDataType dataType, object value1, object value2)
        {
            if (value1.IsNullObject() && value2.IsNullObject()) return 0;
            if (value1.IsNullObject() && !value2.IsNullObject()) return -1;
            if (!value1.IsNullObject() && value2.IsNullObject()) return 1;
            var dataValue1 = dataType.GetSqlValue(value1);
            var dataValue2 = dataType.GetSqlValue(value2);

            switch (dataType)
            {
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                    return string.Compare((string)dataValue1, (string)dataValue2);
                case EnumDataType.Int:
                    return ((int)dataValue1).CompareTo((int)dataValue2);
                case EnumDataType.Date:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
                    return ((DateTime)dataValue1).CompareTo((DateTime)dataValue2);
                case EnumDataType.BigInt:
                    return ((long)dataValue1).CompareTo((long)dataValue2);
                case EnumDataType.Boolean:
                    return ((bool)dataValue1).CompareTo((bool)dataValue2);
                case EnumDataType.Percentage:
                    return ((float)dataValue1).CompareTo((float)dataValue2);
                case EnumDataType.Decimal:
                    return ((decimal)dataValue1).CompareTo((decimal)dataValue2);
                default: return 0;
            }
        }

        public static bool StringContains(this object value1, object value2)
        {
            if (value1 == null || value2 == null) return false;
            return value1.ToString().Contains(value2.ToString());
        }

        public static bool StringStartsWith(this object value1, object value2)
        {
            if (value1 == null || value2 == null) return false;
            return value1.ToString().StartsWith(value2.ToString());
        }

        public static bool StringEndsWith(this object value1, object value2)
        {
            if (value1 == null || value2 == null) return false;
            return value1.ToString().EndsWith(value2.ToString());
        }

        public static object ConvertValueByType(this string value, EnumDataType dataType)
        {
            if (string.IsNullOrEmpty(value)) return null;
            switch (dataType)
            {
                case EnumDataType.Boolean:
                    return value.Trim().ToLower() == true.ToString().ToLower() || value.Trim() == "1";
                case EnumDataType.Date:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
                    return DateTime.Parse(value);
                case EnumDataType.Percentage:
                    return double.Parse(value);
                case EnumDataType.Decimal:
                    return decimal.Parse(value);
                case EnumDataType.Int:
                    return int.Parse(value);
                case EnumDataType.BigInt:
                    return long.Parse(value);
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    return value;
            }
        }

        public static object ConvertValueByType(this string value, Type type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value)) return null;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                    if (string.IsNullOrWhiteSpace(value)) return null;
                }

                if (type == typeof(bool))
                    return string.IsNullOrWhiteSpace(value) ? false : value.Trim().ToLower() == true.ToString().ToLower() || value.Trim() == "1";
                if (type == typeof(DateTime))
                    return string.IsNullOrWhiteSpace(value) ? default : DateTime.Parse(value);
                if (type == typeof(double))
                    return string.IsNullOrWhiteSpace(value) ? default : double.Parse(value);
                if (type == typeof(decimal))
                    return string.IsNullOrWhiteSpace(value) ? default : decimal.Parse(value);
                if (type == typeof(int))
                    return string.IsNullOrWhiteSpace(value) ? default : int.Parse(value);
                if (type == typeof(long))
                    return string.IsNullOrWhiteSpace(value) ? default : long.Parse(value);

                return value;
            }
            catch (Exception ex)
            {

                throw new Exception($"Lỗi convert dữ liệu {value} sang kiểu {type.Name}: {ex.Message}", ex);
            }
        }


        private static Hashtable dbTypeTable;

        public static SqlDbType ConvertToDbType(this Type t)
        {
            if (dbTypeTable == null)
            {
                dbTypeTable = new Hashtable();
                dbTypeTable.Add(typeof(bool), SqlDbType.Bit);
                dbTypeTable.Add(typeof(short), SqlDbType.SmallInt);
                dbTypeTable.Add(typeof(int), SqlDbType.Int);
                dbTypeTable.Add(typeof(long), SqlDbType.BigInt);
                dbTypeTable.Add(typeof(double), SqlDbType.Float);
                dbTypeTable.Add(typeof(decimal), SqlDbType.Decimal);
                dbTypeTable.Add(typeof(string), SqlDbType.NVarChar);
                dbTypeTable.Add(typeof(DateTime), SqlDbType.DateTime);
                dbTypeTable.Add(typeof(byte[]), SqlDbType.VarBinary);
                dbTypeTable.Add(typeof(Guid), SqlDbType.UniqueIdentifier);
            }
            SqlDbType dbtype;
            try
            {
                dbtype = (SqlDbType)dbTypeTable[t];
            }
            catch
            {
                dbtype = SqlDbType.Variant;
            }
            return dbtype;
        }

        public static List<T> ConvertData<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                var props = from p in temp.GetProperties()
                            group p by p.Name into g
                            select g.OrderByDescending(t => t.DeclaringType == typeof(T)).First();

                foreach (PropertyInfo pro in props)
                {
                    if (pro.Name == column.ColumnName && dr[column.ColumnName] != DBNull.Value)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public static List<NonCamelCaseDictionary> ConvertData(this DataTable data)
        {
            var lst = new List<NonCamelCaseDictionary>();
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];
                var dic = new NonCamelCaseDictionary();
                foreach (DataColumn c in data.Columns)
                {
                    var v = row[c];
                    if (v != null && v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?))
                    {
                        var vInDateTime = (v as DateTime?).GetUnix();
                        dic.Add(c.ColumnName, vInDateTime);
                    }
                    else
                    {
                        dic.Add(c.ColumnName, row[c]);
                    }
                }
                lst.Add(dic);
            }
            return lst;
        }

        public static Dictionary<string, (object value, Type type)> ConvertFirstRowData(this DataTable data)
        {
            var result = new Dictionary<string, (object value, Type type)>();

            DataRow row = null;
            if (data.Rows.Count > 0)
            {
                row = data.Rows[0];
            }

            foreach (DataColumn c in data.Columns)
            {

                if (row == null)
                {
                    result.Add(c.ColumnName, (null, c.DataType));
                    continue;
                }

                var v = row[c];

                if (v != null && v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?))
                {
                    var vInDateTime = (v as DateTime?).GetUnix();
                    result.Add(c.ColumnName, (vInDateTime, c.DataType));
                }
                else
                {
                    result.Add(c.ColumnName, (row[c], c.DataType));
                }
            }

            return result;
        }

        public static NonCamelCaseDictionary ToNonCamelCaseDictionary(this Dictionary<string, (object value, Type type)> values)
        {
            var result = new NonCamelCaseDictionary();
            foreach (var data in values)
            {
                result.Add(data.Key, data.Value.value);
            }

            return result;
        }

        public static string ConvertToUnSign2(this string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }

        public static string NormalizeAsInternalName(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            s = s.ConvertToUnSign2();
            s = s.ToLower().Trim();
            return Regex.Replace(s, "[^a-zA-Z0-9\\.\\-]", "");
        }

        public static bool IsVndColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.THANH_TIEN_VND_PREFIX.ToLower());
        }

        public static bool IsTkCoColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.TAI_KHOAN_CO_PREFIX.ToLower());
        }
        public static bool IsTkNoColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.TAI_KHOAN_NO_PREFIX.ToLower());
        }

        public static string VndSumName(this string columnName)
        {
            return $"{AccountantConstants.SUM_RECIPROCAL_PREFIX}{columnName}";
        }
        public static bool IsNgoaiTeColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.THANH_TIEN_NGOAI_TE_PREFIX.ToLower());
        }

        public static IList<CategoryFieldNameModel> GetFieldNameModels<T>(int? byType = null)
        {
            var fields = new List<CategoryFieldNameModel>();
            foreach (var prop in typeof(T).GetProperties())
            {
                var attrs = prop.GetCustomAttributes<System.ComponentModel.DataAnnotations.DisplayAttribute>();

                var title = string.Empty;
                var groupName = "Trường dữ liệu";
                int? type = null;

                if (attrs != null && attrs.Count() > 0)
                {
                    title = attrs.First().Name;
                    groupName = attrs.First().GroupName;
                }
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = prop.Name;
                }

                var types = prop.GetCustomAttributes<FieldDataTypeAttribute>();
                if (types != null && types.Count() > 0)
                {
                    type = types.First().Type;
                }
                if (byType.HasValue && type.HasValue && byType.Value != type.Value)
                {
                    continue;
                }

                var fileMapping = new CategoryFieldNameModel()
                {
                    GroupName = groupName,
                    CategoryFieldId = prop.Name.GetHashCode(),
                    FieldName = prop.Name,
                    FieldTitle = title,
                    Type = type,
                    RefCategory = null
                };

                bool isPrimitiveType = prop.PropertyType.IsPrimitive || prop.PropertyType.IsValueType || (prop.PropertyType == typeof(string));

                if (prop.PropertyType.IsClass && !isPrimitiveType)
                {

                    MethodInfo method = typeof(Utils).GetMethod(nameof(Utils.GetFieldNameModels));
                    MethodInfo generic = method.MakeGenericMethod(prop.PropertyType);
                    var childFields = (IList<CategoryFieldNameModel>)generic.Invoke(null, null);

                    fileMapping.RefCategory = new CategoryNameModel()
                    {
                        CategoryCode = prop.PropertyType.Name,
                        CategoryId = prop.PropertyType.Name.GetHashCode(),
                        CategoryTitle = title,
                        Fields = childFields
                    };

                }

                fields.Add(fileMapping);
            }

            return fields;
        }

        public static string GetPhysicalFilePath(this string filePath, AppSetting appSetting)
        {
            filePath = filePath.Replace('\\', '/');

            while (filePath.StartsWith('.') || filePath.StartsWith('/'))
            {
                filePath = filePath.TrimStart('/').TrimStart('.');
            }

            return appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\') + "/" + filePath;
        }


    }
}
