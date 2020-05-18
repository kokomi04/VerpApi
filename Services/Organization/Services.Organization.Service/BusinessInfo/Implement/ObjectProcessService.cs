using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Organization.Model.BusinessInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Service;

namespace Services.Organization.Service.BusinessInfo.Implement
{
    public class ObjectProcessService : IObjectProcessService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        public ObjectProcessService(OrganizationDBContext organizationContext
            , ILogger logger
            , IActivityLogService activityLogService)
        {
            _organizationContext = organizationContext;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public IList<ObjectProcessInfoModel> ObjectProcessList()
        {
            return EnumExtensions.GetEnumMembers<EnumObjectProcessType>()
                .Select(e => new ObjectProcessInfoModel()
                {
                    ObjectProcessTypeId = e.Enum,
                    ObjectProcessTypeName = e.Description
                })
                .ToList();
        }

        public async Task<IList<ObjectProcessInfoStepModel>> ObjectProcessSteps(EnumObjectProcessType objectProcessTypeId)
        {
            var steps = _organizationContext.ObjectProcessStep.AsNoTracking().Where(s => s.ObjectProcessStepId == (int)objectProcessTypeId).ToList();

            var stepIds = steps.Select(s => s.ObjectProcessStepId).ToList();

            var dependSteps = (await _organizationContext.ObjectProcessStepDepend.AsNoTracking().Where(s => stepIds.Contains(s.ObjectProcessStepId)).ToListAsync())
                .GroupBy(d => d.ObjectProcessStepId)
                .ToDictionary(d => d.Key, d => d.Select(p => p.DependObjectProcessStepId).ToList());

            var stepUsers = (await _organizationContext.ObjectProcessStepUser.AsNoTracking().Where(s => stepIds.Contains(s.ObjectProcessStepId)).ToListAsync())
                .GroupBy(d => d.ObjectProcessStepId)
                .ToDictionary(d => d.Key, d => d.Select(p => p.UserId).ToList());

            var data = new List<ObjectProcessInfoStepModel>();

            return steps.Select(s =>
            {

                dependSteps.TryGetValue(s.ObjectProcessStepId, out var dependStepIds);
                stepUsers.TryGetValue(s.ObjectProcessStepId, out var userIds);
                return new ObjectProcessInfoStepModel()
                {
                    ClientObjectProcessStepId = s.ObjectProcessStepId,
                    ObjectProcessStepId = s.ObjectProcessStepId,
                    ObjectProcessStepName = s.ObjectProcessStepName,
                    DependClientObjectProcessStepIds = dependStepIds,
                    UserIds = userIds
                };
            })
            .ToList();
        }

        public async Task<bool> ObjectProcessUpdate(EnumObjectProcessType objectProcessTypeId, IList<ObjectProcessInfoStepModel> data)
        {
            var duplicateClient = data.GroupBy(d => d.ClientObjectProcessStepId).Where(g => g.Count() > 1).FirstOrDefault();

            if (duplicateClient != null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, duplicateClient.First().ObjectProcessStepName + " trùng ID");
            }

            var stepIds = data.Where(s => s.ObjectProcessStepId.HasValue).Select(s => s.ObjectProcessStepId.Value).ToList();


            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {

                var existedSteps = (await _organizationContext.ObjectProcessStep.Where(s => s.ObjectProcessTypeId == (int)objectProcessTypeId).ToListAsync())
                    .ToDictionary(s => s.ObjectProcessStepId, s => s);

                var clientObjectStep = new Dictionary<int, ObjectProcessStep>();
                var newSteps = new List<ObjectProcessStep>();
                var keepStepIds = new List<int>();
                foreach (var step in data)
                {
                    ObjectProcessStep stepInfo = null;
                    if (step.ObjectProcessStepId.HasValue)
                    {
                        if (!existedSteps.TryGetValue(step.ObjectProcessStepId.Value, out stepInfo) || stepInfo.ObjectProcessTypeId != step.ClientObjectProcessStepId)
                        {
                            await trans.RollbackAsync();
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Bước " + step.ObjectProcessStepName + " không tồn tại hoặc dữ liệu không đúng");
                        }

                        stepInfo.ObjectProcessStepName = step.ObjectProcessStepName;
                        keepStepIds.Add(stepInfo.ObjectProcessTypeId);
                    }
                    else
                    {
                        stepInfo = new ObjectProcessStep()
                        {
                            ObjectProcessTypeId = (int)objectProcessTypeId,
                            ObjectProcessStepName = step.ObjectProcessStepName
                        };

                        newSteps.Add(stepInfo);
                    }

                    clientObjectStep.Add(step.ClientObjectProcessStepId, stepInfo);
                }

                if (newSteps.Count > 0)
                {
                    await _organizationContext.ObjectProcessStep.AddRangeAsync(newSteps);
                }

                var removeSteps = existedSteps.Where(s => !keepStepIds.Contains(s.Key)).Select(s => s.Value).ToList();
                foreach (var removeStep in removeSteps)
                {
                    removeStep.IsDeleted = true;
                }

                await _organizationContext.SaveChangesAsync();

                _organizationContext.ObjectProcessStepDepend.RemoveRange(_organizationContext.ObjectProcessStepDepend.Where(s => stepIds.Contains(s.ObjectProcessStepId)));

                _organizationContext.ObjectProcessStepUser.RemoveRange(_organizationContext.ObjectProcessStepUser.Where(s => stepIds.Contains(s.ObjectProcessStepId)));

                var lstObjectProcessStepDepend = new List<ObjectProcessStepDepend>();
                var lstObjectProcessStepUser = new List<ObjectProcessStepUser>();
                foreach (var item in data)
                {
                    var stepInfo = clientObjectStep[item.ClientObjectProcessStepId];
                    foreach (var dependStepId in item.DependClientObjectProcessStepIds)
                    {
                        lstObjectProcessStepDepend.Add(new ObjectProcessStepDepend()
                        {
                            ObjectProcessStepId = stepInfo.ObjectProcessStepId,
                            DependObjectProcessStepId = clientObjectStep[dependStepId].ObjectProcessStepId
                        });
                    }

                    foreach (var userId in item.UserIds)
                    {
                        lstObjectProcessStepUser.Add(new ObjectProcessStepUser()
                        {
                            ObjectProcessStepId = stepInfo.ObjectProcessStepId,
                            UserId = userId
                        });
                    }
                }

                if (lstObjectProcessStepDepend.Count > 0)
                {
                    await _organizationContext.ObjectProcessStepDepend.AddRangeAsync(lstObjectProcessStepDepend);
                }
                if (lstObjectProcessStepUser.Count > 0)
                {
                    await _organizationContext.ObjectProcessStepUser.AddRangeAsync(lstObjectProcessStepUser);
                }

                await _organizationContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ObjectProcess, (int)objectProcessTypeId, $"Cập nhật quy trình xử lý {objectProcessTypeId}", data.JsonSerialize());

                return true;
            }
        }
    }
}
