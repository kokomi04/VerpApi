using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcProto.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Service.Activity;
    
namespace GrpcService.Service
{
    public class InternalActivityLogService: InternalActivityLog.InternalActivityLogBase
    {
        private readonly IActivityService _activityService;

        public InternalActivityLogService(IActivityService activityService)
        {
            _activityService = activityService;
        }

        public override Task<ActivityReponse> Log(ActivityInput request, ServerCallContext context)
        {
            _activityService.CreateActivityAsync(new VErp.Infrastructure.ServiceCore.Model.ActivityInput
            {
                UserId = request.UserId,
                ActionId = (VErp.Commons.Enums.MasterEnum.EnumAction)request.ActionId,
                ObjectTypeId = (VErp.Commons.Enums.MasterEnum.EnumObjectType)request.ObjectTypeId,
                MessageTypeId = (VErp.Commons.Enums.EnumMessageType)request.MessageTypeId,
                ObjectId = request.ObjectId,
                Message = request.Message,
                Data = request.Data
            });
            return  Task.FromResult(new ActivityReponse { IsSuccess = true }) ;
        }
    }
}
