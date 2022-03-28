using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Commons.ObjectExtensions.Extensions;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Commons.Library
{

    public static class Utils
    {

        public static ILoggerFactory LoggerFactory;

        public static ILoggerFactory DefaultLoggerFactory
        {
            get
            {
                return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });
            }
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

              

        public static string GetObjectKey(EnumObjectType objectTypeId, long objectId)
        {
            return $"{((int)objectTypeId)}_{objectId}";
        }

        private static Expression<Func<T, object>> ToMemberOf<T>(this string name)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyOrField = Expression.PropertyOrField(parameter, name);
            var unaryExpression = Expression.MakeUnary(ExpressionType.Convert, propertyOrField, typeof(object));

            return Expression.Lambda<Func<T, object>>(unaryExpression, parameter);
        }

        public static IOrderedQueryable<T> SortByFieldName<T>(this IQueryable<T> query, string filedName, bool asc)
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
            if (propertyInfo == null) return query.OrderBy(s => 1);

            var ex = propertyInfo.Name.ToMemberOf<T>();
            return asc ? query.OrderBy(ex) : query.OrderByDescending(ex);
        }


      
        /*

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
        }*/

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

      
      


        public static string GetPhysicalFilePath(this string filePath, AppSetting appSetting)
        {
            filePath = filePath.Replace('\\', '/');

            while (filePath.StartsWith('.') || filePath.StartsWith('/'))
            {
                filePath = filePath.TrimStart('/').TrimStart('.');
            }

            return appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\') + "/" + filePath;
        }
    

        public static void ValidateCodeSpecialCharactors(this string code, string desc = "")
        {
            if (string.IsNullOrEmpty(code))
                return;

            var regEx = new Regex("^([0-9a-zA-Z])(([0-9a-zA-Z_\\.\\,\\/\\-#\\+&])*([0-9a-zA-Z]))*$", RegexOptions.Multiline);
            if (!regEx.IsMatch(code))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Mã {code} {desc} không hợp lệ, mã phải bắt đầu và kết thúc bởi chữ hoặc số, không được chứa dấu cách trống và ký tự đặc biệt (ngoài A-Z, 0-9 và \\.,/-_#&+)");
            }
        }

     
        public static IEnumerable<string> GetRangeOfAllowValueForBoolean()
        {
            return RangeValueConstants.RANGE_OF_ALLOW_VALUE_FOR_BOOLEAN_TRUE.Concat(RangeValueConstants.RANGE_OF_ALLOW_VALUE_FOR_BOOLEAN_FALSE);
        }

        /// <summary>
        /// Trả về "true" nếu giá trị nằm trong phạm vị giá trị cho phép của kiểu boolean. Và ngược lại
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasValueInRangeOfAllowValueForBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            return RangeValueConstants.RANGE_OF_ALLOW_VALUE_FOR_BOOLEAN_TRUE.Concat(RangeValueConstants.RANGE_OF_ALLOW_VALUE_FOR_BOOLEAN_FALSE).Select(x => x.NormalizeAsInternalName()).Contains(value.NormalizeAsInternalName());
        }

        public static bool IsRangeOfAllowValueForBooleanTrueValue(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return RangeValueConstants.RANGE_OF_ALLOW_VALUE_FOR_BOOLEAN_TRUE.Select(x => x.NormalizeAsInternalName()).Contains(value.NormalizeAsInternalName());
        }


    }
}
