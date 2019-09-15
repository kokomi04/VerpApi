using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ApiCore.Model
{
    public class ApiResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public static implicit operator ApiResponse(Enum code)
        {
            return new ApiResponse()
            {
                Code = code.GetErrorCodeString(),
                Message = code.GetEnumDescription()
            };
        }

        public static implicit operator ApiResponse(ServiceResult result)
        {
            return new ApiResponse()
            {
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
                Code = GeneralCode.Success.GetErrorCodeString(),
                Data = data
            };
        }

        public static implicit operator ApiResponse<T>(ServiceResult<T> result)
        {
            return new ApiResponse<T>()
            {
                Code = result.Code.GetErrorCodeString(),
                Data = result.Data,
                Message = result.Message
            };
        }
    }

}
