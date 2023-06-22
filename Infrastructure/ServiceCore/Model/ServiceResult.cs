﻿using Newtonsoft.Json;
using System;
using System.Diagnostics;
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

        public static implicit operator ServiceResult<T>((Enum code, string message) err)
        {
            return new ServiceResult<T>()
            {
                Code = err.code,
                Message = err.message
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
    public static class ServiceResultExtensions
    {
        public static bool IsSuccessCode(this ServiceResult result)
        {
            return result.Code.IsSuccess();
        }

        public static bool IsSuccessCode<T>(this ServiceResult<T> result)
        {
            return result.Code.IsSuccess();
        }
    }

    public class ApiErrorResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }

        [JsonProperty("exception$")]
        public ExceptionModel ExceptionDebug { get; set; }
        public object AdditionData { get; set; }
    }

    public class ExceptionModel
    {
        public string Message { get; }
        public string StackTrace { get; }
        public string Source { get; }
        public ExceptionModel InnerException { get; }

        public ExceptionModel(Exception ex)
        {
            Message = ex?.Message;
            StackTrace = ex?.StackTrace;
            Source = ex?.Source;
            if (ex.InnerException != null)
                InnerException = new ExceptionModel(ex.InnerException);
        }
    }
    //public class ApiErrorResponse<T> : ApiErrorResponse
    //{

    //}

}
