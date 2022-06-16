using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using VErp.Commons.Constants;
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



        public static IList<CategoryFieldNameModel> GetFieldNameModels<T>(int? byType = null, bool forExport = false, bool ignoreCheckField = false, string preFix = "")
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

            var sortOrder = 1;
            foreach (var prop in typeof(T).GetProperties())
            {
                var attrs = prop.GetCustomAttributes<DisplayAttribute>();

                var title = string.Empty;
                var groupName = "Trường dữ liệu";

                var order = sortOrder++;
                int? type = null;

                if (attrs != null && attrs.Count() > 0)
                {
                    title = attrs.First().Name;
                    groupName = attrs.First().GroupName;

                    if (attrs.First().GetOrder() > 0)
                        order = attrs.First().Order;
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


                var isRequired = prop.GetCustomAttribute<RequiredAttribute>();

                if (prop.GetCustomAttribute<FieldDataNestedObjectAttribute>() != null && prop.PropertyType.IsClass())
                {
                    MethodInfo method = typeof(ExcelUtils).GetMethod(nameof(ExcelUtils.GetFieldNameModels));
                    MethodInfo generic = method.MakeGenericMethod(prop.PropertyType);
                    var nestedFields = (IList<CategoryFieldNameModel>)generic.Invoke(null, new[] { (object)null, false, true, prop.Name });
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
                        var childFields = (IList<CategoryFieldNameModel>)generic.Invoke(null, new[] { (object)null, false, true, "" });

                        fileMapping.RefCategory = new CategoryNameModel()
                        {
                            CategoryCode = prop.PropertyType.Name,
                            //CategoryId = prop.PropertyType.Name.GetHashCode(),
                            CategoryTitle = title,
                            Fields = childFields
                        };

                    }

                    fields.Add(fileMapping);

                }
            }


            return fields.OrderBy(f => f.SortOrder).ToList();
        }
    }
}
