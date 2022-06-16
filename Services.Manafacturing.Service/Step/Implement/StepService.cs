﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Step;
using StepEntity = VErp.Infrastructure.EF.ManufacturingDB.Step;

namespace VErp.Services.Manafacturing.Service.Step.Implement
{
    public class StepService : IStepService
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
            if (req.StepGroupId == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn chưa thuộc nhóm nào.");

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var entity = _mapper.Map<StepEntity>(req);
                await _manufacturingDBContext.Step.AddAsync(entity);
                await _manufacturingDBContext.SaveChangesAsync();

                var detail = _mapper.Map<List<StepDetail>>(req.StepDetail);
                detail.ForEach(x => { x.StepId = entity.StepId; });

                await _manufacturingDBContext.StepDetail.AddRangeAsync(detail);

                await _manufacturingDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.Step, entity.StepId, $"Tạo danh mục công đoạn '{entity.StepName}'", entity.JsonSerialize());
                await trans.CommitAsync();

                return entity.StepId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "CreateStep");
                throw;
            }

        }

        public async Task<bool> DeleteStep(int stepId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var step = _manufacturingDBContext.Step.Include(x => x.ProductionStep).FirstOrDefault(x => x.StepId == stepId);
                var stepDetail = await _manufacturingDBContext.StepDetail.Where(x => x.StepId == step.StepId).ToListAsync();
                if (step == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);
                if (step.ProductionStep.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa do nó đang được sử dụng trong quy trình sản xuất");

                step.IsDeleted = true;
                stepDetail.ForEach(x => { x.IsDeleted = true; });

                await _manufacturingDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.Step, step.StepId, $"Xóa danh mục công đoạn '{step.StepName}'", step.JsonSerialize());
                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeleteStep");
                throw;
            }

        }

        public async Task<PageData<StepModel>> GetListStep(string keyWord, int page, int size)
        {
            keyWord = (keyWord ?? "").Trim();

            var query = _manufacturingDBContext.Step.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyWord))
                query = query.Where(x => x.StepName.Contains(keyWord));

            var total = await query.CountAsync();

            query = query.OrderBy(x => x.IsHide).ThenBy(x => x.SortOrder);

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }

            var data = await query.ProjectTo<StepModel>(_mapper.ConfigurationProvider)
                            .ToListAsync();
            return (data, total);
        }

        public async Task<StepModel> GetStep(int stepId)
        {
            var step = await _manufacturingDBContext.Step.AsNoTracking()
                .ProjectTo<StepModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.StepId == stepId);
            if (step == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            return step;
        }

        public async Task<IList<StepModel>> GetStepByArrayId(int[] arrayId)
        {
            return await _manufacturingDBContext.Step.AsNoTracking()
                 .Where(x => arrayId.Contains(x.StepId))
                 .ProjectTo<StepModel>(_mapper.ConfigurationProvider)
                 .ToListAsync();
        }

        public async Task<bool> UpdateStep(int stepId, StepModel req)
        {
            if (req.StepGroupId == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn chưa thuộc nhóm nào.");

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var step = _manufacturingDBContext.Step.FirstOrDefault(x => x.StepId == stepId);
                var stepDetail = await _manufacturingDBContext.StepDetail.Where(x => x.StepId == step.StepId).ToListAsync();
                if (step == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                _mapper.Map(req, step);
                foreach (var detail in stepDetail)
                {
                    var m = req.StepDetail.FirstOrDefault(x => x.StepDetailId == detail.StepDetailId);
                    if (m != null)
                        _mapper.Map(m, detail);
                    else detail.IsDeleted = true;
                }

                var newStepDetail = _mapper.Map<List<StepDetail>>(req.StepDetail.Where(n => !stepDetail.Select(x => x.StepDetailId).Contains(n.StepDetailId)).ToList());
                newStepDetail.ForEach(x => { x.StepId = step.StepId; });
                await _manufacturingDBContext.StepDetail.AddRangeAsync(newStepDetail);

                await _manufacturingDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.Step, step.StepId, $"Cập nhật danh mục công đoạn '{step.StepName}'", step.JsonSerialize());
                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateStep");
                throw;
            }

        }
    }
}
