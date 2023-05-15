using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;


namespace VErp.Commons.Library
{
    public static class ConvertUtils
    {


        public static bool Convertible(this EnumDataType oldType, EnumDataType newType)
        {
            if (oldType.IsTimeType() && !newType.IsTimeType() && newType != EnumDataType.Text)
            {
                return false;
            }

            return true;
        }


        public static object GetSqlValueAtTimezone(this EnumDataType dataType, object value, int? timeZoneOffset)
        {
            return GetSqlValueWithCustomTimezone(dataType, value, timeZoneOffset);
        }

        public static object GetSqlValue(this EnumDataType dataType, object value)
        {
            return GetSqlValueWithCustomTimezone(dataType, value, null);
        }

        public static string GetDefaultValueRawSqlStringWithQuote(this EnumDataType dataType)
        {
            switch (dataType)
            {
                case EnumDataType.Text:
                    return "''";
                case EnumDataType.Int:
                    return "0";

                case EnumDataType.Date:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
                    return "'1900-01-01'";

                case EnumDataType.PhoneNumber: return "";
                case EnumDataType.Email: return "";
                case EnumDataType.Boolean:
                    return "0";
                case EnumDataType.Percentage:
                    return "0";
                case EnumDataType.BigInt:
                    return "0";
                case EnumDataType.Decimal:
                    return "0";
                default: return null;
            }
        }

        private static object GetSqlValueWithCustomTimezone(this EnumDataType dataType, object value, int? timeZoneOffset)
        {
            if (value.IsNullOrEmptyObject()) return DBNull.Value;

            switch (dataType)
            {
                case EnumDataType.Text:
                    return value?.ToString()?.Trim();
                case EnumDataType.Int:
                    int intValue;
                    try
                    {
                        intValue = Convert.ToInt32(value);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value?.JsonSerialize()} sang kiểu int");
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
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value?.JsonSerialize()} sang kiểu ngày tháng");
                    }

                    return dateValue.UnixToDateTime(timeZoneOffset).Value;

                case EnumDataType.PhoneNumber: return value?.ToString()?.Trim();
                case EnumDataType.Email: return value?.ToString()?.Trim();
                case EnumDataType.Boolean:
                    bool boolValue;
                    try
                    {
                        boolValue = Convert.ToBoolean(value);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value?.JsonSerialize()} sang kiểu logic");
                    }
                    return boolValue;
                case EnumDataType.Percentage:
                    float percentValue;
                    if (!float.TryParse(value.ToString()?.Trim(), out percentValue))// || percentValue < -100 || percentValue > 100)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value?.JsonSerialize()} sang kiểu phần trăm");
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
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value?.JsonSerialize()} sang kiểu long");
                    }
                    return longValue;
                case EnumDataType.Decimal:
                    decimal decimalValue;
                    if (!decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimalValue))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value?.JsonSerialize()} sang kiểu decimal");
                    }
                    return decimalValue;
                default: return value?.ToString()?.Trim();
            }
        }


        public static EnumDataType GetDataType(this Type objectType)
        {
            if (objectType.IsNumber()) return EnumDataType.Decimal;

            if (objectType == typeof(string)) return EnumDataType.Text;

            if (objectType == typeof(bool)) return EnumDataType.Boolean;

            if (objectType == typeof(DateTime)) return EnumDataType.Date;


            return EnumDataType.Text;
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

                case EnumDataType.BigInt:
                case EnumDataType.Decimal:
                    return EnumExcelType.Number;
                case EnumDataType.Percentage:
                    return EnumExcelType.Percentage;
                case EnumDataType.Date:
                    return EnumExcelType.DateTime;
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                default:
                    return EnumExcelType.String;
            }
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



        public static object ConvertValueByType(this string value, EnumDataType dataType)
        {
            if (string.IsNullOrEmpty(value)) return null;
            switch (dataType)
            {
                case EnumDataType.Boolean:
                    return StringToBool(value);
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
                    return StringToBool(value);
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

        private static bool StringToBool(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? false :
                value.Trim().ToLower() == true.ToString().ToLower()
                || value.Trim() == "1"
                || value.Trim().NormalizeAsInternalName() == "co"
                || value.Trim().NormalizeAsInternalName() == "yes";
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
                    if (pro.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase) && dr[column.ColumnName] != DBNull.Value)
                    {
                        try
                        {
                            var v = ConvertObjectToSpecialType(pro, dr[column.ColumnName]);
                            pro.SetValue(obj, v, null);

                        }
                        catch (Exception)
                        {

                            throw;
                        }

                    }
                    else
                        continue;
                }
            }
            return obj;
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


        private static object ConvertObjectToSpecialType(PropertyInfo pro, object v)
        {
            var type = pro.PropertyType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = pro.PropertyType.GenericTypeArguments[0];
            }

            if (type == typeof(long) && v?.GetType() == typeof(DateTime))
            {
                return (v as DateTime?).GetUnix();
            }

            if (type.IsEnum)
            {
                return Enum.ToObject(type, v);
            }

            return Convert.ChangeType(v, type);
        }
        public static List<T> ConvertDataModel<T>(this DataTable data) where T : NonCamelCaseDictionary, new()
        {
            var lst = new List<T>();
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];
                var dic = new T();
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
                    if (v == DBNull.Value)
                    {
                        dic.Add(c.ColumnName, null);
                        continue;
                    }

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


        public static IList<NonCamelCaseDictionary> ToNonCamelCaseDictionaryList<TSource>(this IList<TSource> values, Action<TSource, NonCamelCaseDictionary, IList<NonCamelCaseDictionary>> rowAction = null) where TSource : class
        {
            var result = new List<NonCamelCaseDictionary>();
            var props = typeof(TSource).GetProperties();

            foreach (var r in values)
            {
                var row = new NonCamelCaseDictionary();
                foreach (var p in props)
                {
                    row.Add(p.Name, p.GetValue(r));
                }

                result.Add(row);

                if (rowAction != null)
                    rowAction(r, row, result);
            }

            return result;
        }

        public static T ToCustomDictionary<T, TSource, TKey, TElement>(this IEnumerable<TSource> source, T initData, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where T : Dictionary<TKey, TElement>, new()
        {
            //var result = new T();// Activator.CreateInstance<T>(source.Count());
            foreach (var item in source)
            {
                initData.Add(keySelector.Invoke(item), elementSelector.Invoke(item));
            }

            return initData;

        }

        public static IList<TSource> ToIList<TSource>(this IEnumerable<TSource> source)
        {
            return new List<TSource>(source);
        }

    }
}
