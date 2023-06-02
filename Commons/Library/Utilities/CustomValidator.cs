using OpenXmlPowerTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Utilities
{
    public enum EnumCustomNormalizeAndValidateOption
    {
        All = int.MaxValue & ~IgnoreRequired,
        ValidateEnum = 1,
        ValidateModel = 2,
        TrimString = 4,
        IgnoreRequired = 8
    }
    public class CustomValidationResult : ValidationResult
    {
        public CustomValidationResult(string errorMessage) : base(errorMessage)
        {
        }

        public CustomValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames)
        {
        }

        public CustomValidationResult(ValidationResult validationResult) : base(validationResult)
        {

        }

        public string DisplayName { get; set; }
    }

    public class CustomValidator
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> _typeProps = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static ConcurrentDictionary<PropertyInfo, ValidationAttribute[]> _propValidations = new ConcurrentDictionary<PropertyInfo, ValidationAttribute[]>();
        private static ConcurrentDictionary<PropertyInfo, DisplayAttribute> _propDisplayNames = new ConcurrentDictionary<PropertyInfo, DisplayAttribute>();


        public static bool TryNormalizeAndValidateObject(object obj, ICollection<CustomValidationResult> results, EnumCustomNormalizeAndValidateOption multilOptions)
        {
            var context = new ValidationContext(obj);
            var opt = new NormalizeAndValidateOption(multilOptions);
            return NormalizeAndValidateObject(obj, string.Empty, results, context, new List<ValidationAttribute>(), opt);
        }

        private static bool NormalizeAndValidateObject(object obj, string propName, ICollection<CustomValidationResult> results, ValidationContext context, IList<ValidationAttribute> validationAttributes, NormalizeAndValidateOption opt)
        {

            var validateResults = new List<ValidationResult>();
            if (opt.ValidateModel && !Validator.TryValidateValue(obj, context, validateResults, validationAttributes))
            {
                foreach (var r in validateResults)
                {
                    results.Add(new CustomValidationResult(r) { DisplayName = context.DisplayName });
                }

                return false;
            }

            if (obj == null)
            {
                return true;
            }

            var type = obj.GetType();
            bool isPrimitiveType = ObjectUtils.IsPrimitiveType(type);

            if (isPrimitiveType)
            {
                if (opt.ValidateEnum && type.IsEnum)
                {
                    if (!type.IsEnumDefined(obj))
                    {
                        results.Add(new CustomValidationResult("Invalid enum " + propName + "=" + obj, new[] { propName }) { DisplayName = context.DisplayName });

                        return false;
                    }
                }
            }
            else
            {
                if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                {
                    foreach (object item in (dynamic)obj)
                    {
                        if (!NormalizeAndValidateObject(item, propName, results, context, validationAttributes, opt))
                        {
                            return false;
                        }
                    }

                }
                else
                {
                    if (type.IsClass && !(obj is Microsoft.AspNetCore.Http.HeaderDictionary))
                    {
                        if (!_typeProps.TryGetValue(type, out var props))
                        {
                            props = type.GetProperties();
                            _typeProps.TryAdd(type, props);
                        }

                        foreach (var p in props)
                        {

                            var v = p.GetValue(obj);

                            if (opt.TrimString && v != null && p.CanWrite)
                            {
                                var vType = v.GetType();
                                if (vType == typeof(string))
                                {
                                    p.SetValue(obj, v?.ToString()?.Trim());
                                }
                            }

                            if (!_propValidations.TryGetValue(p, out var attrs))
                            {
                                attrs = p.GetCustomAttributes(true)
                                    .Where(a => typeof(ValidationAttribute).IsAssignableFrom(a.GetType()))
                                    .Select(a => (ValidationAttribute)a)
                                    .ToArray();
                                _propValidations.TryAdd(p, attrs);
                            }

                            if (!_propDisplayNames.TryGetValue(p, out var displayAttr))
                            {
                                displayAttr = p.GetCustomAttribute<DisplayAttribute>(true);
                                _propDisplayNames.TryAdd(p, displayAttr);
                            }


                            var newContext = new ValidationContext(obj) { MemberName = p.Name };
                            if (!string.IsNullOrWhiteSpace(displayAttr?.Name))
                            {
                                newContext.DisplayName = displayAttr?.Name;
                            }
                            var newValidationAttributes = attrs;
                            if (opt.IgnoreRequired)
                            {
                                newValidationAttributes = newValidationAttributes.Where(a => !typeof(RequiredAttribute).IsAssignableFrom(a.GetType())).ToArray();
                            }

                            if (!NormalizeAndValidateObject(v, propName + "." + p.Name, results, newContext, newValidationAttributes, opt))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }


        private class NormalizeAndValidateOption
        {
            private EnumCustomNormalizeAndValidateOption _multilOptions;
            public NormalizeAndValidateOption(EnumCustomNormalizeAndValidateOption multilOptions)
            {
                _multilOptions = multilOptions;
                ValidateEnum = Convert(EnumCustomNormalizeAndValidateOption.ValidateEnum);
                ValidateModel = Convert(EnumCustomNormalizeAndValidateOption.ValidateModel);
                TrimString = Convert(EnumCustomNormalizeAndValidateOption.TrimString);
                IgnoreRequired = Convert(EnumCustomNormalizeAndValidateOption.IgnoreRequired);
            }
            private bool Convert(EnumCustomNormalizeAndValidateOption option)
            {
                return (_multilOptions & option) == option;
            }

            public bool ValidateEnum { get; }
            public bool ValidateModel { get; }
            public bool TrimString { get; }
            public bool IgnoreRequired { get; }
        }

    }
}
