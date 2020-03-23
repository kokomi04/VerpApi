using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ServiceCore.Model;
using Newtonsoft.Json;

namespace VErp.Infrastructure.ApiCore.Model
{
    public class ApiResponse
    {
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }

        public static implicit operator ApiResponse(Enum code)
        {
            return new ApiResponse()
            {
                StatusCode = code.GetEnumStatusCode(),
                Code = code.GetErrorCodeString(),
                Message = code.GetEnumDescription()
            };
        }

        public static implicit operator ApiResponse(ServiceResult result)
        {
            return new ApiResponse()
            {
                StatusCode = result.Code.GetEnumStatusCode(),
                Code = result.Code.GetErrorCodeString(),
                Message = result.Message
            };
        }
    }

    
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public static implicit operator ApiResponse<T>(T data)
        {
            return new ApiResponse<T>()
            {
                StatusCode = HttpStatusCode.OK,
                Code = GeneralCode.Success.GetErrorCodeString(),
                Data = data
            };
        }


        public static implicit operator ApiResponse<T>(ServiceResult<T> result)
        {
            return new ApiResponse<T>()
            {
                StatusCode = result.Code.GetEnumStatusCode(),
                Code = result.Code.GetErrorCodeString(),
                Data = result.Data,
                Message = result.Message
            };
        }

        public static implicit operator ApiResponse<T>(Enum code)
        {
            return new ApiResponse<T>()
            {
                StatusCode = code.GetEnumStatusCode(),
                Code = code.GetErrorCodeString(),
                Message = code.GetEnumDescription()
            };
        }
    }

}
