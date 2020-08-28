using ActivityLogDB;
using Grpc.Core;
using GrpcProto.Protos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Services.Grpc.Service
{
    class InternalActivityLogService : InternalActivityLog.InternalActivityLogBase
    {
        private readonly ActivityLogDBContext _activityLogContext;
        private readonly ILogger<InternalActivityLogService> _logger;

        public InternalActivityLogService(ActivityLogDBContext activityLogContext
            , ILogger<InternalActivityLogService> logger)
        {
            _activityLogContext = activityLogContext;
            _logger = logger;
        }

        public override async Task<ActivityResponses> Log(ActivityInput request, ServerCallContext context)
        {
            using (var trans = await _activityLogContext.Database.BeginTransactionAsync())
            {
                var activity = new UserActivityLog()
                {
                    UserId = request.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    ActionId = (int)request.ActionId,
                    ObjectTypeId = (int)request.ObjectTypeId,
                    MessageTypeId = (int)request.MessageTypeId,
                    ObjectId = request.ObjectId,
                    Message = request.Message
                };

                await _activityLogContext.UserActivityLog.AddAsync(activity);
                await _activityLogContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(request.Data))
                {
                    var change = new UserActivityLogChange()
                    {
                        UserActivityLogId = activity.UserActivityLogId,
                        ObjectChange = request.Data,//changeLog
                    };

                    await _activityLogContext.UserActivityLogChange.AddAsync(change);
                }
                await _activityLogContext.SaveChangesAsync();

                trans.Commit();
            }

            return await Task.FromResult(new ActivityResponses { IsSuccess = true });
        }
    }
}
