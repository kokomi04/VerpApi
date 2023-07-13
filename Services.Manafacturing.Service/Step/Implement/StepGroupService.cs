using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.Step;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Step.Implement
{
    public class StepGroupService : IStepGroupService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public StepGroupService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<StepGroupService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.StepGroup);
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> CreateStepGroup(StepGroupModel req)
        {
            var entity = _mapper.Map<StepGroup>(req);
            _manufacturingDBContext.StepGroup.Add(_mapper.Map<StepGroup>(entity));
            await _manufacturingDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => StepActivityLogMessage.CreateStepGroup)
                   .MessageResourceFormatDatas(entity.StepGroupName)
                   .ObjectId(entity.StepGroupId)
                   .ObjectType(EnumObjectType.StepGroup)
                   .JsonData(entity)
                   .CreateLog();
            return entity.StepGroupId;
        }

        public async Task<bool> DeleteStepGroup(int stepGroupId)
        {
            var groupStep = _manufacturingDBContext.StepGroup.Include(x => x.Step).FirstOrDefault(x => x.StepGroupId == stepGroupId);
            if (groupStep == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            if (groupStep.Step.Count > 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa nhóm!. Đang tồn tại công đoạn trong nhóm");

            groupStep.IsDeleted = true;
            await _manufacturingDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => StepActivityLogMessage.DeleteStepGroup)
                   .MessageResourceFormatDatas(groupStep.StepGroupName)
                   .ObjectId(groupStep.StepGroupId)
                   .ObjectType(EnumObjectType.StepGroup)
                   .JsonData(groupStep)
                   .CreateLog();
            return true;
        }

        public async Task<PageData<StepGroupModel>> GetListStepGroup(string keyWord, int page, int size)
        {
            keyWord = (keyWord ?? "").Trim();

            var query = _manufacturingDBContext.StepGroup.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyWord))
                query = query.Where(x => x.StepGroupName.Contains(keyWord));

            var total = await query.CountAsync();

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }

            var data = await query.ProjectTo<StepGroupModel>(_mapper.ConfigurationProvider)
                            .ToListAsync();
            return (data, total);
        }

        public async Task<bool> UpdateStepGroup(int stepGroupId, StepGroupModel req)
        {
            var destInfo = _manufacturingDBContext.StepGroup.FirstOrDefault(x => x.StepGroupId == stepGroupId);
            if (destInfo == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            _mapper.Map(req, destInfo);

            await _manufacturingDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => StepActivityLogMessage.UpdateStepGroup)
                   .MessageResourceFormatDatas(destInfo.StepGroupName)
                   .ObjectId(destInfo.StepGroupId)
                   .ObjectType(EnumObjectType.StepGroup)
                   .JsonData(destInfo)
                   .CreateLog();
            return true;
        }
    }
}
