﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Verp.Resources.GlobalObject;

namespace VErp.Commons.GlobalObject.DataAnnotationsExtensions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GreaterThanAttribute : DataTypeAttribute
    {
        public object Min { get { return _min; } }

        private readonly double _min;

        public GreaterThanAttribute(int min) : base("min")
        {
            _min = min;
        }

        public GreaterThanAttribute(double min) : base("min")
        {
            _min = min;
        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessage == null && ErrorMessageResourceName == null)
            {
                ErrorMessage = ValidatorResources.GreaterThanAttribute_Invalid;
            }

            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _min);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            double valueAsDouble;

            var isDouble = double.TryParse(Convert.ToString(value), out valueAsDouble);

            return isDouble && valueAsDouble > _min;
        }
    }
}
