using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service.Facade;
using VErp.Services.Manafacturing.Service.Step;

namespace VErp.Services.Manafacturing.Service
{
    public interface ITargetProductivityService
    {
        Task<int> AddTargetProductivity(TargetProductivityModel model);
        Task<bool> DeleteTargetProductivity(int targetProductivityId);
        Task<TargetProductivityModel> GetTargetProductivity(int targetProductivityId);
        Task<IList<TargetProductivityModel>> Search(string keyword, int page, int size);
        Task<bool> UpdateTargetProductivity(int targetProductivityId, TargetProductivityModel model);

        CategoryNameModel GetFieldDataForMapping();

        Task<IList<TargetProductivityDetailModel>> ParseDetails(ImportExcelMapping mapping, Stream stream);
    }

    public class TargetProductivityService : ITargetProductivityService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IStepService _stepService;

        public TargetProductivityService(ManufacturingDBContext manufacturingDBContext, IActivityLogService activityLogService
            , ILogger<TargetProductivityService> logger, IMapper mapper, IStepService stepService)
        {
            _manufacturingDBContext = manufacturingDBContext;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _stepService = stepService;
        }

        public async Task<int> AddTargetProductivity(TargetProductivityModel model)
        {
            if (_manufacturingDBContext.TargetProductivity.Any(x => x.TargetProductivityCode == model.TargetProductivityCode))
                throw new BadRequestException(GeneralCode.InvalidParams, "Đã tồn tại mã năng suất mục tiêu trong hệ thống");

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (model.IsDefault)
                    await RemoveDefaultTargetProductivity();

                var entity = _mapper.Map<TargetProductivity>(model);
                await _manufacturingDBContext.TargetProductivity.AddAsync(entity);
                await _manufacturingDBContext.SaveChangesAsync();

                var eDetails = _mapper.Map<IList<TargetProductivityDetail>>(model.TargetProductivityDetail);
                foreach (var item in eDetails) item.TargetProductivityId = entity.TargetProductivityId;

                await _manufacturingDBContext.TargetProductivityDetail.AddRangeAsync(eDetails);
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                return entity.TargetProductivityId;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("TargetProductivityService.AddTargetProductivity", ex);
                throw ex;
            }
        }

        public async Task<bool> UpdateTargetProductivity(int targetProductivityId, TargetProductivityModel model)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (!_manufacturingDBContext.TargetProductivity.Any(x => x.TargetProductivityId == targetProductivityId))
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                var entity = _manufacturingDBContext.TargetProductivity.FirstOrDefault(x => x.TargetProductivityId == targetProductivityId);
                var details = _manufacturingDBContext.TargetProductivityDetail.Where(x => x.TargetProductivityId == targetProductivityId).ToList();

                if (entity.TargetProductivityCode != model.TargetProductivityCode && _manufacturingDBContext.TargetProductivity.Any(x => x.TargetProductivityCode == model.TargetProductivityCode))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Đã tồn tại mã năng suất mục tiêu trong hệ thống");

                if (model.IsDefault)
                    await RemoveDefaultTargetProductivity();

                model.TargetProductivityId = targetProductivityId;
                foreach (var d in model.TargetProductivityDetail)
                {
                    d.TargetProductivityId = targetProductivityId;
                }

                _mapper.Map(model, entity);

                foreach (var detail in details)
                {
                    var mDetail = model.TargetProductivityDetail.FirstOrDefault(x => x.TargetProductivityDetailId == detail.TargetProductivityDetailId);
                    if (mDetail != null)
                        _mapper.Map(mDetail, detail);
                    else detail.IsDeleted = true;
                }

                var eNewDetails = _mapper.Map<IList<TargetProductivityDetail>>(model.TargetProductivityDetail.Where(x => x.TargetProductivityDetailId <= 0).ToList());

                await _manufacturingDBContext.TargetProductivityDetail.AddRangeAsync(eNewDetails);
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("TargetProductivityService.UpdateTargetProductivity", ex);
                throw ex;
            }
        }


        public async Task<bool> DeleteTargetProductivity(int targetProductivityId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (!_manufacturingDBContext.TargetProductivity.Any(x => x.TargetProductivityId == targetProductivityId))
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                var entity = _manufacturingDBContext.TargetProductivity.FirstOrDefault(x => x.TargetProductivityId == targetProductivityId);
                var details = _manufacturingDBContext.TargetProductivityDetail.Where(x => x.TargetProductivityId == targetProductivityId).ToList();

                foreach (var detail in details)
                    detail.IsDeleted = true;

                entity.IsDeleted = true;

                await _manufacturingDBContext.SaveChangesAsync();

                if (entity.IsDefault)
                    await SetDefaultTargetProductivity();

                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("TargetProductivityService.DeleteTargetProductivity", ex);
                throw ex;
            }
        }

        public async Task<TargetProductivityModel> GetTargetProductivity(int targetProductivityId)
        {
            try
            {
                if (!_manufacturingDBContext.TargetProductivity.Any(x => x.TargetProductivityId == targetProductivityId))
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                var entity = await _manufacturingDBContext.TargetProductivity.FirstOrDefaultAsync(x => x.TargetProductivityId == targetProductivityId);
                var details = await _manufacturingDBContext.TargetProductivityDetail.Where(x => x.TargetProductivityId == targetProductivityId).ToListAsync();

                var result = _mapper.Map<TargetProductivityModel>(entity);
                result.TargetProductivityDetail = _mapper.Map<IList<TargetProductivityDetailModel>>(details);

                return result;
            }
            catch (System.Exception ex)
            {
                _logger.LogError("TargetProductivityService.DeleteTargetProductivity", ex);
                throw ex;
            }
        }

        public async Task<IList<TargetProductivityModel>> Search(string keyword, int page, int size)
        {
            var query = _manufacturingDBContext.TargetProductivity.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.TargetProductivityCode.Contains(keyword));

            query = size > 0 ? query.Skip((page - 1) * size).Take(size) : query;

            return await query.ProjectTo<TargetProductivityModel>(_mapper.ConfigurationProvider).ToListAsync();
        }


        public CategoryNameModel GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "TargetProductivity",
                CategoryTitle = "TargetProductivity",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<TargetProductivityDetailModel>();
            result.Fields = fields;
            return result;
        }

        public Task<IList<TargetProductivityDetailModel>> ParseDetails(ImportExcelMapping mapping, Stream stream)
        {
            return new TargetProductivityParseExcelFacade(_stepService)
                 .ParseInvoiceDetails(mapping, stream);
        }



        private async Task<bool> RemoveDefaultTargetProductivity()
        {
            var defaults = await _manufacturingDBContext.TargetProductivity.Where(x => x.IsDefault == true).ToListAsync();
            defaults.ForEach(x => x.IsDefault = false);
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        private async Task<bool> SetDefaultTargetProductivity()
        {
            var target = await _manufacturingDBContext.TargetProductivity.FirstOrDefaultAsync(x => x.IsDefault == false);
            target.IsDefault = true;
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

    }
}