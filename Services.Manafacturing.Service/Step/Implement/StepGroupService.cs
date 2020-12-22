using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Step.Implement
{
    public class StepGroupService : IStepGroupService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public StepGroupService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<StepGroupService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> CreateStepGroup(StepGroupModel req)
        {
            var entity = _mapper.Map<StepGroup>(req);
            _manufacturingDBContext.StepGroup.Add(_mapper.Map<StepGroup>(entity));
            await _manufacturingDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.StepGroup, entity.StepGroupId, $"Tạo nhóm danh mục công đoạn '{entity.StepGroupName}'", entity.JsonSerialize());
            return entity.StepGroupId;
        }

        public async Task<bool> DeleteStepGroup(int stepGroupId)
        {
            var groupStep = _manufacturingDBContext.StepGroup.Include(x => x.Step).FirstOrDefault(x => x.StepGroupId == stepGroupId);
            if (groupStep == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            if (groupStep.Step.Count > 0)
                throw new BadRequestException(GeneralCode.GeneralError, "Không thể xóa nhóm!. Đang tồn tại công đoạn trong nhóm");

            groupStep.IsDeleted = true;
            await _manufacturingDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.StepGroup, groupStep.StepGroupId, $"Xóa nhóm danh mục công đoạn '{groupStep.StepGroupName}'", groupStep.JsonSerialize());
            return true;
        }

        public async Task<PageData<StepGroupModel>> GetListStepGroup(string keyWord, int page, int size)
        {
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
            await _activityLogService.CreateLog(EnumObjectType.StepGroup, destInfo.StepGroupId, $"Cập nhật nhóm danh mục công đoạn '{destInfo.StepGroupName}'", destInfo.JsonSerialize());
            return true;
        }
    }
}
