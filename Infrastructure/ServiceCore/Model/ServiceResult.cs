using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Infrastructure.ServiceCore.Model
{

    public class ServiceResult
    {
        public Enum Code { get; set; }
        public string Message { get; set; }
        public static implicit operator ServiceResult(Enum code)
        {
            return new ServiceResult()
            {
                Code = code,
                Message = code.GetEnumDescription()
            };
        }

        public static implicit operator ServiceResult((Enum code, string message) data)
        {
            return new ServiceResult()
            {
                Code = data.code,
                Message = data.message
            };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; set; }

        public static implicit operator ServiceResult<T>(Enum code)
        {
            return new ServiceResult<T>()
            {
                Code = code,
                Message = code.GetEnumDescription()
            };
        }
        public static implicit operator ServiceResult<T>(T data)
        {
            return new ServiceResult<T>()
            {
                Code = GeneralCode.Success,
                Data = data
            };
        }

    }
}
