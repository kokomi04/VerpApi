using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.Extensions;

namespace VErp.Commons.GlobalObject
{
    public class BadRequestException : Exception
    {
        public Enum Code { get; set; }

        public BadRequestException(Enum errorCode) : base(EnumExtensions.GetEnumDescription(errorCode))
        {
            this.Code = errorCode;
        }

        public BadRequestException(Enum errorCode, string message) : base(message)
        {
            this.Code = errorCode;
        }

        public BadRequestException(string message) : base(message)
        {
            this.Code = GeneralCode.InvalidParams;
        }

        public BadRequestException(Enum errorCode, object[] param) : base(string.Format(EnumExtensions.GetEnumDescription(errorCode), param))
        {
            this.Code = errorCode;
        }

        public BadRequestException(Enum errorCode, string message, IDictionary<string, object> data) : base(message)
        {
            this.Code = errorCode;
            foreach (var item in data)
            {
                this.Data.Add(item.Key, item.Value);
            }
        }
    }


    public static class BadRequestExceptionExtensions
    {
        public static BadRequestException BadRequest(this Enum code)
        {
            return new BadRequestException(code);
        }

        public static BadRequestException BadRequest(this string message)
        {
            return new BadRequestException(message);
        }

        public static BadRequestException BadRequest(this (Enum code, string message) error)
        {
            return new BadRequestException(error.code, error.message);
        }   

        public static BadRequestException BadRequestFormat(this string str, params object[] args)
        {
            return new BadRequestException(str.Format(args));
        }

        public static BadRequestFormatBuilder BadFormat(this string str)
        {
            return new BadRequestFormatBuilder(str);
        }
    }

    public class BadRequestFormatBuilder
    {
        public readonly List<object> objs = new List<object>();
        private readonly string formatString;

        public BadRequestFormatBuilder(string formatString)
        {
            this.formatString = formatString;
        }

        public BadRequestFormatBuilder Add(object agr)
        {
            objs.Add(agr);
            return this;
        }

        public BadRequestException Build()
        {
            return formatString.BadRequestFormat(objs);
        }
    }
}
