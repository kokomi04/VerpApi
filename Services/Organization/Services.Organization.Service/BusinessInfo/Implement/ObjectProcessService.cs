using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Organization.Model.BusinessInfo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.ObjectProcess;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace Services.Organization.Service.BusinessInfo.Implement
{
    public class ObjectProcessService : IObjectProcessService
    {
        private readonly OrganizationDBContext _organizationContext;

        private readonly ObjectActivityLogFacade _objectProcessActivityLog;

        public ObjectProcessService(OrganizationDBContext organizationContext
            , ILogger<ObjectProcessService> logger
            , IActivityLogService activityLogService)
        {
            _organizationContext = organizationContext;

            _objectProcessActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ObjectProcessStep);
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

        public async Task<IList<ObjectProcessInfoStepListModel>> ObjectProcessSteps(EnumObjectProcessType objectProcessTypeId)
        {
            var steps = _organizationContext.ObjectProcessStep.AsNoTracking().Where(s => s.ObjectProcessTypeId == (int)objectProcessTypeId).ToList();

            var stepIds = steps.Select(s => s.ObjectProcessStepId).ToList();

            var dependSteps = (await _organizationContext.ObjectProcessStepDepend.AsNoTracking().Where(s => stepIds.Contains(s.ObjectProcessStepId)).ToListAsync())
                .GroupBy(d => d.ObjectProcessStepId)
                .ToDictionary(d => d.Key, d => d.Select(p => p.DependObjectProcessStepId).ToList());

            var stepUsers = (await _organizationContext.ObjectProcessStepUser.AsNoTracking().Where(s => stepIds.Contains(s.ObjectProcessStepId)).ToListAsync())
                .GroupBy(d => d.ObjectProcessStepId)
                .ToDictionary(d => d.Key, d => d.Select(p => p.UserId).ToList());

            var data = new List<ObjectProcessInfoStepListModel>();

            return steps.Select(s =>
            {

                dependSteps.TryGetValue(s.ObjectProcessStepId, out var dependStepIds);
                stepUsers.TryGetValue(s.ObjectProcessStepId, out var userIds);
                return new ObjectProcessInfoStepListModel()
                {
                    SortOrder = s.SortOrder,
                    ObjectProcessStepId = s.ObjectProcessStepId,
                    ObjectProcessStepName = s.ObjectProcessStepName,
                    DependObjectProcessStepIds = dependStepIds,
                    UserIds = userIds
                };
            })
            .ToList();
        }

        public async Task<int> ObjectProcessStepCreate(EnumObjectProcessType objectProcessTypeId, ObjectProcessInfoStepModel model)
        {
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                var steps = _organizationContext.ObjectProcessStep.AsNoTracking().Where(s => s.ObjectProcessTypeId == (int)objectProcessTypeId).ToList();
                if (model.DependObjectProcessStepIds?.Except(steps.Select(s => s.ObjectProcessStepId))?.Count() > 0)
                {
                    await trans.RollbackAsync();
                    throw new BadRequestException(GeneralCode.InvalidParams);
                }

                var info = new ObjectProcessStep()
                {
                    ObjectProcessTypeId = (int)objectProcessTypeId,
                    ObjectProcessStepName = model.ObjectProcessStepName,

                    SortOrder = model.SortOrder
                };
                await _organizationContext.ObjectProcessStep.AddAsync(info);
                await _organizationContext.SaveChangesAsync();

                if (model.DependObjectProcessStepIds?.Count > 0)
                {
                    await _organizationContext.ObjectProcessStepDepend.AddRangeAsync(model.DependObjectProcessStepIds.Select(d => new ObjectProcessStepDepend()
                    {
                        ObjectProcessStepId = info.ObjectProcessStepId,
                        DependObjectProcessStepId = d
                    }));
                }

                if (model.UserIds?.Count > 0)
                {
                    await _organizationContext.ObjectProcessStepUser.AddRangeAsync(model.UserIds.Select(userId => new ObjectProcessStepUser()
                    {
                        ObjectProcessStepId = info.ObjectProcessStepId,
                        UserId = userId
                    }));
                }

                await _organizationContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _objectProcessActivityLog.LogBuilder(() => ObjectProcessActivityLogMessage.Create)
                    .MessageResourceFormatDatas($"{info.ObjectProcessStepName} {objectProcessTypeId.GetEnumDescription()}")
                    .ObjectId(info.ObjectProcessStepId)
                    .JsonData(info.JsonSerialize())
                    .CreateLog();

                return info.ObjectProcessStepId;
            }
        }

        public async Task<bool> ObjectProcessStepUpdate(EnumObjectProcessType objectProcessTypeId, int objectProcessStepId, ObjectProcessInfoStepModel model)
        {
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                var steps = _organizationContext.ObjectProcessStep.AsNoTracking().Where(s => s.ObjectProcessTypeId == (int)objectProcessTypeId).ToList();
                if (model.DependObjectProcessStepIds?.Except(steps.Select(s => s.ObjectProcessStepId))?.Count() > 0)
                {
                    await trans.RollbackAsync();
                    throw new BadRequestException(GeneralCode.InvalidParams);
                }

                var info = await _organizationContext.ObjectProcessStep.FirstOrDefaultAsync(s => s.ObjectProcessStepId == objectProcessStepId);
                if (info == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound);
                }

                info.SortOrder = model.SortOrder;
                info.ObjectProcessStepName = model.ObjectProcessStepName;

                var depends = _organizationContext.ObjectProcessStepDepend.Where(d => d.ObjectProcessStepId == objectProcessStepId);
                _organizationContext.ObjectProcessStepDepend.RemoveRange(depends);

                if (model.DependObjectProcessStepIds?.Count > 0)
                {
                    await _organizationContext.ObjectProcessStepDepend.AddRangeAsync(model.DependObjectProcessStepIds.Select(d => new ObjectProcessStepDepend()
                    {
                        ObjectProcessStepId = info.ObjectProcessStepId,
                        DependObjectProcessStepId = d
                    }));
                }

                var users = _organizationContext.ObjectProcessStepUser.Where(d => d.ObjectProcessStepId == objectProcessStepId);
                _organizationContext.ObjectProcessStepUser.RemoveRange(users);

                if (model.UserIds?.Count > 0)
                {
                    await _organizationContext.ObjectProcessStepUser.AddRangeAsync(model.UserIds.Select(userId => new ObjectProcessStepUser()
                    {
                        ObjectProcessStepId = info.ObjectProcessStepId,
                        UserId = userId
                    }));
                }

                await _organizationContext.SaveChangesAsync();

                await trans.CommitAsync();


                await _objectProcessActivityLog.LogBuilder(() => ObjectProcessActivityLogMessage.Update)
                    .MessageResourceFormatDatas($"{info.ObjectProcessStepName} {objectProcessTypeId.GetEnumDescription()}")
                    .ObjectId(info.ObjectProcessStepId)
                    .JsonData(info.JsonSerialize())
                    .CreateLog();
                return true;
            }
        }

        public async Task<bool> ObjectProcessStepDelete(EnumObjectProcessType objectProcessTypeId, int objectProcessStepId)
        {
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                var steps = _organizationContext.ObjectProcessStep.AsNoTracking().Where(s => s.ObjectProcessTypeId == (int)objectProcessTypeId).ToList();

                var info = await _organizationContext.ObjectProcessStep.FirstOrDefaultAsync(s => s.ObjectProcessStepId == objectProcessStepId);
                if (info == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound);
                }

                var depends = _organizationContext.ObjectProcessStepDepend.Where(d => d.ObjectProcessStepId == objectProcessStepId || d.DependObjectProcessStepId == objectProcessStepId);
                _organizationContext.ObjectProcessStepDepend.RemoveRange(depends);


                var users = _organizationContext.ObjectProcessStepUser.Where(d => d.ObjectProcessStepId == objectProcessStepId);
                _organizationContext.ObjectProcessStepUser.RemoveRange(users);

                await _organizationContext.SaveChangesAsync();

                await trans.CommitAsync();


                await _objectProcessActivityLog.LogBuilder(() => ObjectProcessActivityLogMessage.Delete)
                    .MessageResourceFormatDatas($"{info.ObjectProcessStepName} {objectProcessTypeId.GetEnumDescription()}")
                    .ObjectId(info.ObjectProcessStepId)
                    .JsonData(info.JsonSerialize())
                    .CreateLog();

                return true;
            }
        }
    }
}
