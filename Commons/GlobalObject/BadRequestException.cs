﻿using System;
using System.Collections.Generic;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.Extensions;

namespace VErp.Commons.GlobalObject
{
    public class BadRequestException : Exception
    {
        public Enum Code { get; set; }
        public object AdditionData { get; set; }

        public BadRequestException(Enum errorCode) : base(EnumExtensions.GetEnumDescription(errorCode))
        {
            this.Code = errorCode;
        }

        public BadRequestException(Enum errorCode, string message) : base(message)
        {
            this.Code = errorCode;
        }
        public BadRequestException(object additionData, Enum errorCode, string message) : base(message)
        {
            this.Code = errorCode;
            this.AdditionData = additionData;
        }
        public BadRequestException(string message) : base(message)
        {
            this.Code = GeneralCode.InvalidParams;
        }

        public BadRequestException(Exception innerException, string message) : base(message, innerException)
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

        public static BadRequestException BadRequest(this Enum code, string message)
        {
            return new BadRequestException(code, message);
        }

        public static BadRequestException BadRequestFormat(this Enum code, string messageFormat, params object[] args)
        {
            return new BadRequestException(code, messageFormat.Format(args));
        }

        public static BadRequestException BadRequestFormatWithData(this Enum code, object additionData, string messageFormat, params object[] args)
        {
            return new BadRequestException(additionData, code, messageFormat.Format(args));
        }


        public static BadRequestException BadRequestDescriptionFormat(this Enum code, params object[] args)
        {
            return new BadRequestException(code, args);
        }

        public static BadRequestException BadRequest(this string message)
        {
            return new BadRequestException(message);
        }
        public static BadRequestException BadRequestWithData(this string message, object additionData)
        {
            return new BadRequestException(additionData, GeneralCode.InvalidParams, message);
        }

        public static BadRequestException BadRequest(this string message, Enum code)
        {
            return new BadRequestException(code, message);
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
            return formatString.BadRequestFormat(objs.ToArray());
        }
    }
}
