using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

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
    }


    public static class BadRequestExceptionExtensions
    {
        public static BadRequestException BadRequest(this Enum code)
        {
            return new BadRequestException(code);
        }

        public static BadRequestException BadRequest(this (Enum code, string message) error)
        {
            return new BadRequestException(error.code, error.message);
        }
    }
}
