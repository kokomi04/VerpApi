using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Infrastructure.ApiCore.Model
{
    public class ApiResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public static implicit operator ApiResponse(Enum code)
        {
            return new ApiResponse()
            {
                Code = Convert.ToInt32(code)
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
                Code = (int)GeneralCode.Success,
                Data = data
            };
        }
    }

}
