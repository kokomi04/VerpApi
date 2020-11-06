using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
using StepEnity = VErp.Infrastructure.EF.ManufacturingDB.Step;

namespace VErp.Services.Manafacturing.Service.Step.Implement
{
    public class StepService: IStepService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public StepService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<StepService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> CreateStep(StepModel req)
        {
            var entity = _mapper.Map<StepEnity>(req);
            _manufacturingDBContext.Step.Add(_mapper.Map<StepEnity>(entity));
            await _manufacturingDBContext.SaveChangesAsync();

            _activityLogService.CreateLog(EnumObjectType.Step, entity.StepId, $"Tạo danh mục công đoạn '{entity.StepName}'", entity.JsonSerialize());
            return entity.StepGroupId;
        }

        public async Task<bool> DeleteStep(int stepId)
        {
            var destInfo = _manufacturingDBContext.Step.FirstOrDefault(x => x.StepId == stepId);
            if (destInfo == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            destInfo.IsDeleted = true;
            await _manufacturingDBContext.SaveChangesAsync();
            _activityLogService.CreateLog(EnumObjectType.Step, destInfo.StepId, $"Xóa danh mục công đoạn '{destInfo.StepName}'", destInfo.JsonSerialize());
            return true;
        }

        public async Task<PageData<StepModel>> GetListStep(string keyWord, int page, int size)
        {
            var query = _manufacturingDBContext.Step.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyWord))
                query = query.Where(x => x.StepName.Contains(keyWord));

            var total = await query.CountAsync();

            var data = query.ProjectTo<StepModel>(_mapper.ConfigurationProvider)
                            .Skip((page - 1) * size).Take(size).ToList();
            return (data, total);
        }

        public async Task<bool> UpdateStep(int stepId, StepModel req)
        {
            var destInfo = _manufacturingDBContext.Step.FirstOrDefault(x => x.StepId == stepId);
            if (destInfo == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            _mapper.Map(req, destInfo);

            await _manufacturingDBContext.SaveChangesAsync();
            _activityLogService.CreateLog(EnumObjectType.Step, destInfo.StepId, $"Cập nhật danh mục công đoạn '{destInfo.StepName}'", destInfo.JsonSerialize());
            return true;
        }
    }
}
