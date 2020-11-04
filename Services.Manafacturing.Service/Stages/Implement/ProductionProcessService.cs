using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Stages;

namespace VErp.Services.Manafacturing.Service.Stages.Implement
{
    public class ProductionProcessService : IProductionProcessService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> CreateProductionStages(int productId, ProductionStagesModel req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var stages = _mapper.Map<ProductionStages>(req);
                    await _manufacturingDBContext.ProductionStages.AddAsync(stages);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var inOutStages =  req.InOutStages.Select(io => _mapper.Map<ProductionStagesDetail>(io)).ToList();
                    inOutStages.ForEach(io => io.ProductionStagesId = stages.ProductionStagesId);
                    await _manufacturingDBContext.ProductionStagesDetail.AddRangeAsync(inOutStages);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, stages.ProductionStagesId,
                        $"Tạo mới công đoạn {req.ProductionStagesId} của sản phẩm {req.ProductId}", req.JsonSerialize());
                    return stages.ProductionStagesId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductionStagesById");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteProductionStagesById(int productId, int stagesId)
        {
            var stagesInfo = await _manufacturingDBContext.ProductionStages
                                   .Where(s => s.ProductionStagesId == stagesId)
                                   .FirstOrDefaultAsync();
            if (stagesInfo == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundStages);

            var inOutStages = await _manufacturingDBContext.ProductionStagesDetail
                                    .Where(d => d.ProductionStagesId == stagesInfo.ProductionStagesId)
                                    .ToListAsync();
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    stagesInfo.IsDeleted = true;
                    inOutStages.ForEach(s => s.IsDeleted = true);

                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, stagesInfo.ProductionStagesId,
                        $"Xóa công đoạn {stagesInfo.ProductionStagesId} của sản phẩm {stagesInfo.ProductId}", stagesInfo.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "DeleteProductionStagesById");
                    throw;
                }
            }
        }

        public async Task<bool> GenerateStagesMapping(int productId, List<ProductionStagesMappingModel> req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var oldMapping = _manufacturingDBContext.ProductionStagesMapping.Where(m => m.ProductId == productId).ToList();
                    if (oldMapping.Count > 0)
                    {
                        _manufacturingDBContext.ProductionStagesMapping.RemoveRange(oldMapping);
                        await _manufacturingDBContext.SaveChangesAsync();
                    }

                    _manufacturingDBContext.ProductionStagesMapping.AddRange(_mapper.Map<List<ProductionStagesMapping>>(req));
                    await _manufacturingDBContext.SaveChangesAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, productId,
                        $"Khởi tạo mapping công đoạn của sản phẩm {productId}", req.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "DeleteProductionStagesById");
                    throw;
                }
            }
            
        }

        public async Task<ProductionProcessModel> GetProductionProcessByProductId(int productId)
        {
            var stagesInfos = await _manufacturingDBContext.ProductionStages.AsNoTracking()
                                        .Where(s => s.ProductId == productId)
                                        .ProjectTo<ProductionStagesModel>(_mapper.ConfigurationProvider)
                                        .ToListAsync();
            var stagesMapping = await _manufacturingDBContext.ProductionStagesMapping.AsNoTracking()
                                        .Where(s => s.ProductId == productId)
                                        .ProjectTo<ProductionStagesMappingModel>(_mapper.ConfigurationProvider)
                                        .ToListAsync();

            return new ProductionProcessModel
            {
                ProductionStages = stagesInfos,
                StagesMapping = stagesMapping
            };
        }

        public async Task<ProductionStagesModel> GetProductionStagesById(int productId, int stagesId)
        {
            var stagesInfo = await _manufacturingDBContext.ProductionStages.AsNoTracking()
                                    .Where(s => s.ProductionStagesId == stagesId)
                                    .ProjectTo<ProductionStagesModel>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync();
            if (stagesInfo == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundStages);

            stagesInfo.InOutStages = await _manufacturingDBContext.ProductionStagesDetail.AsNoTracking()
                                            .Where(d => d.ProductionStagesId == stagesInfo.ProductionStagesId)
                                            .ProjectTo<InOutStagesModel>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return stagesInfo;
        }

        public async Task<bool> UpdateProductionStagesById(int productId, int stagesId, ProductionStagesModel req)
        {
            var stagesInfo = await _manufacturingDBContext.ProductionStages
                                   .Where(s => s.ProductionStagesId == stagesId)
                                   .FirstOrDefaultAsync();
            if (stagesInfo == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundStages);

            var inOutStages = await _manufacturingDBContext.ProductionStagesDetail
                                    .Where(d => d.ProductionStagesId == stagesInfo.ProductionStagesId)
                                    .ToListAsync();
            using(var trans  = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map(req, stagesInfo);

                    foreach(var dest in inOutStages)
                    {
                        var source = req.InOutStages.FirstOrDefault(r => r.ProductionStagesDetailId == dest.ProductionStagesDetailId);
                        if (source == null)
                            throw new BadRequestException(ProductionStagesErrorCode.NotFoundInOutStages);
                        _mapper.Map(source, dest);
                    }

                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, stagesInfo.ProductionStagesId,
                        $"Cập nhật công đoạn {stagesInfo.ProductionStagesId} của sản phẩm {stagesInfo.ProductId}", req.JsonSerialize());
                    return true;
                }catch(Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductionStagesById");
                    throw;
                }
            }

        }
    }
}
