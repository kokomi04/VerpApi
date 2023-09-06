using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.Library.Model;


namespace VErp.Commons.Library
{
    public static class ExcelUtils
    {

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


        public static string GetFullPropertyPath<T, TKey>(Expression<Func<T, TKey>> action)
        {
            var path = action.Body.ToString();
            var firstPoint = path.IndexOf('.');
            if (firstPoint < 0) return "";
            return path.Substring(firstPoint + 1);
        }

        public static string GetFullPropertyPath<T>(Expression<Func<T, object>> action)
        {
            return GetFullPropertyPath<T, object>(action);
        }

        public static string GetFullPropertyPath<T>(this T _, Expression<Func<T, object>> action)
        {
            return GetFullPropertyPath(action);
        }


        public static IList<CategoryFieldNameModel> GetFieldNameModels<T>(int? byType = null, bool forExport = false, bool ignoreCheckField = false, string preFix = "", int parentOrder = 0, IDynamicCategoryHelper categoryHelper = null)
        {
            var fields = new List<CategoryFieldNameModel>();

            if (!forExport && !ignoreCheckField)
            {
                fields.Add(new CategoryFieldNameModel()
                {
                    GroupName = "Dòng dữ liệu",
                    FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
                    FieldTitle = "Cột kiểm tra"
                });
            }

            var sortOrder = parentOrder;
            var props = typeof(T).GetProperties().ToList();

            foreach (var prop in props)
            {
                //var duplicateContainsProp = props.FirstOrDefault(p => p != prop && p.Name.StartsWith(prop.Name));
                //if (duplicateContainsProp != null)
                //{
                //    throw GeneralCode.InvalidParams.BadRequest($"Can not import data with field name contains either. '{duplicateContainsProp.Name}' include '{prop.Name}'");
                //}

                var attrs = prop.GetCustomAttributes<DisplayAttribute>();

                var title = string.Empty;
                var groupName = "Trường dữ liệu";

                var order = sortOrder++;
                int? type = null;

                if (attrs != null && attrs.Count() > 0)
                {
                    var attr = attrs.First();
                    title = attr.Name;
                    if (!string.IsNullOrWhiteSpace(attr.GroupName))
                        groupName = attr.GroupName;

                    if (attrs.First().GetOrder() > 0)
                        order = parentOrder + attrs.First().Order;
                }
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = prop.Name;
                }

                //var types = prop.GetCustomAttributes<FieldDataTypeAttribute>();
                //if (types != null && types.Count() > 0)
                //{
                //    type = types.First().Type;
                //}
                //if (byType.HasValue && type.HasValue && byType.Value != type.Value)
                //{
                //    continue;
                //}


                if (prop.GetCustomAttribute<FieldDataIgnoreAttribute>() != null) continue;

                if (forExport && prop.GetCustomAttribute<FieldDataIgnoreExportAttribute>() != null) continue;

                if (!forExport && prop.GetCustomAttribute<FieldDataIgnoreImportAttribute>() != null) continue;

                var isRequired = prop.GetCustomAttribute<RequiredAttribute>();

                if (prop.GetCustomAttribute<FieldDataNestedObjectAttribute>() != null && prop.PropertyType.IsClass())
                {
                    MethodInfo method = typeof(ExcelUtils).GetMethod(nameof(ExcelUtils.GetFieldNameModels));
                    MethodInfo generic = method.MakeGenericMethod(prop.PropertyType);
                    var nestedFields = (IList<CategoryFieldNameModel>)generic.Invoke(null, new[] { (object)null, false, true, prop.Name, order, categoryHelper });
                    foreach (var f in nestedFields)
                    {
                        fields.Add(new CategoryFieldNameModel()
                        {
                            GroupName = groupName,
                            //CategoryFieldId = prop.Name.GetHashCode(),
                            FieldName = f.FieldName,
                            FieldTitle = f.FieldTitle,
                            IsRequired = f.IsRequired,
                            Type = f.Type,
                            RefCategory = f.RefCategory,
                            SortOrder = f.SortOrder
                        });
                    }
                }
                else
                {

                    var fileMapping = new CategoryFieldNameModel()
                    {
                        GroupName = groupName,
                        //CategoryFieldId = prop.Name.GetHashCode(),
                        FieldName = preFix + prop.Name,
                        FieldTitle = title,
                        IsRequired = isRequired != null,
                        Type = type,
                        RefCategory = null,
                        SortOrder = order
                    };


                    if (prop.PropertyType.IsClass())
                    {

                        MethodInfo method = typeof(ExcelUtils).GetMethod(nameof(ExcelUtils.GetFieldNameModels));
                        MethodInfo generic = method.MakeGenericMethod(prop.PropertyType);
                        var childFields = (IList<CategoryFieldNameModel>)generic.Invoke(null, new[] { (object)null, false, true, "", order, categoryHelper });

                        fileMapping.RefCategory = new CategoryNameModel()
                        {
                            CategoryCode = prop.PropertyType.Name,
                            //CategoryId = prop.PropertyType.Name.GetHashCode(),
                            CategoryTitle = title,
                            Fields = childFields
                        };

                    }

                    var isRefCate = prop.GetCustomAttribute<DynamicCategoryMappingAttribute>();
                    if (isRefCate != null)
                    {
                        var cateFields = categoryHelper.GetReferFields(new[] { isRefCate.CategoryCode }, null).Result;

                        fileMapping.RefCategory = new CategoryNameModel()
                        {
                            CategoryCode = isRefCate.CategoryCode,
                            //CategoryId = prop.PropertyType.Name.GetHashCode(),
                            CategoryTitle = title,
                            Fields = cateFields?.Select(f => new CategoryFieldNameModel()
                            {
                                GroupName = title,
                                //CategoryFieldId = prop.Name.GetHashCode(),
                                FieldName = f.CategoryFieldName,
                                FieldTitle = f.CategoryFieldTitle,
                                IsRequired = isRequired != null,
                                DataTypeId = (EnumDataType)f.DataTypeId,
                                Type = type,
                                RefCategory = null,
                                SortOrder = null
                            }).ToList()
                        };

                    }

                    fields.Add(fileMapping);

                }
            }


            return fields.OrderBy(f => f.SortOrder).ToList();
        }
    }
}
