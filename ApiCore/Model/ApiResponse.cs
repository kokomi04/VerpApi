//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using VErp.Commons.Enums.StandardEnum;
//using VErp.Infrastructure.ServiceCore.Model;
//using Newtonsoft.Json;

//namespace VErp.Infrastructure.ApiCore.Model
//{
//    public class ServiceResult
//    {
//        public string Code { get; set; }
//        public string Message { get; set; }

//        public static implicit operator ServiceResult(Enum code)
//        {
//            return new ServiceResult()
//            {
//                Code = code.GetErrorCodeString(),
//                Message = code.GetEnumDescription()
//            };
//        }

//        public static implicit operator ServiceResult(ServiceResult result)
//        {
//            return new ServiceResult()
//            {
//                Code = result.Code.GetErrorCodeString(),
//                Message = result.Message
//            };
//        }
//    }

    
//    public class ServiceResult<T> : ServiceResult
//    {
//        public T Data { get; set; }

//        public static implicit operator ServiceResult<T>(T data)
//        {
//            return new ServiceResult<T>()
//            {
//                Code = GeneralCode.Success.GetErrorCodeString(),
//                Data = data
//            };
//        }


//        public static implicit operator ServiceResult<T>(ServiceResult<T> result)
//        {
//            return new ServiceResult<T>()
//            {
//                Code = result.Code.GetErrorCodeString(),
//                Data = result.Data,
//                Message = result.Message
//            };
//        }

//        public static implicit operator ServiceResult<T>(Enum code)
//        {
//            return new ServiceResult<T>()
//            {
//                Code = code.GetErrorCodeString(),
//                Message = code.GetEnumDescription()
//            };
//        }
//    }

//}
