using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
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

        public async Task<int> CreateProductionStep(int productId, ProductionStepInfo req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var step = _mapper.Map<ProductionStep>((ProductionStepModel)req);
                    await _manufacturingDBContext.ProductionStep.AddAsync(step);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var rInOutStepLinks = await InsertAndUpdateProductInStep(step.ProductionStepId, req.ProductInSteps);
                    await _manufacturingDBContext.InOutStepLink.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, step.ProductionStepId,
                        $"Tạo mới công đoạn {req.ProductionStepId} của sản phẩm {req.ProductId}", req.JsonSerialize());
                    return step.ProductionStepId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "CreateProductionStep");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteProductionStepById(int productId, int productionStepId)
        {
            var productionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .Include(x=>x.InOutStepLink)
                                   .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundProductionStep);

            var productInSteps = await _manufacturingDBContext.ProductInStep
                .Where(x => productionStep.InOutStepLink.Select(r => r.ProductInStepId)
                .Contains(x.ProductInStepId)).ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    productionStep.IsDeleted = true;
                    productInSteps.ForEach(s => s.IsDeleted = true);

                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, productionStep.ProductionStepId,
                        $"Xóa công đoạn {productionStep.ProductionStepId} của sản phẩm {productionStep.ProductId}", productionStep.JsonSerialize());
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

        public async Task<bool> GenerateProductionStepMapping(int productId, List<ProductionStepLinkModel> req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var oldMapping = _manufacturingDBContext.ProductionStepLink.Where(m => m.ProductId == productId).ToList();
                    if (oldMapping.Count > 0)
                    {
                        _manufacturingDBContext.ProductionStepLink.RemoveRange(oldMapping);
                        await _manufacturingDBContext.SaveChangesAsync();
                    }

                    _manufacturingDBContext.ProductionStepLink.AddRange(_mapper.Map<List<ProductionStepLink>>(req));
                    await _manufacturingDBContext.SaveChangesAsync();
                    trans.Commit();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, productId,
                        $"Khởi tạo mapping công đoạn của sản phẩm {productId}", req.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "GenerateStagesMapping");
                    throw;
                }
            }
            
        }

        public async Task<ProductionProcessInfo> GetProductionProcessByProductId(int productId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                        .Where(s => s.ProductId == productId)
                                        .ProjectTo<ProductionStepModel>(_mapper.ConfigurationProvider)
                                        .ToListAsync();
            var productionStepLinks = await _manufacturingDBContext.ProductionStepLink.AsNoTracking()
                                        .Where(s => s.ProductId == productId)
                                        .ProjectTo<ProductionStepLinkModel>(_mapper.ConfigurationProvider)
                                        .ToListAsync();

            return new ProductionProcessInfo
            {
                ProductionSteps = productionSteps,
                ProductionStepLinks = productionStepLinks
            };
        }

        public async Task<ProductionStepInfo> GetProductionStepById(int productId, int productionStepId)
        {
            var productionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                    .Where(s => s.ProductionStepId == productionStepId)
                                    .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundProductionStep);

            productionStep.ProductInSteps = await _manufacturingDBContext.InOutStepLink.AsNoTracking()
                                            .Where(d => d.ProductionStepId == productionStep.ProductionStepId)
                                            .Include(x => x.ProductionStep)
                                            .ProjectTo<ProductInStepInfo>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return productionStep;
        }

        public async Task<bool> UpdateProductionStagesById(int productId, int productionStepId, ProductionStepInfo req)
        {
            var sProductionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .FirstOrDefaultAsync();
            if (sProductionStep == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundProductionStep);

            var dInOutStepLinks = await _manufacturingDBContext.InOutStepLink
                                    .Where(x => x.ProductionStepId == sProductionStep.ProductionStepId)
                                    .ToListAsync();

            using(var trans  = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map((ProductionStepModel)req, sProductionStep);

                    var rInOutStepLinks = await InsertAndUpdateProductInStep(sProductionStep.ProductionStepId, req.ProductInSteps);
                    _manufacturingDBContext.InOutStepLink.RemoveRange(dInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await _manufacturingDBContext.InOutStepLink.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    _activityLogService.CreateLog(EnumObjectType.ProductionStages, sProductionStep.ProductionStepId,
                        $"Cập nhật công đoạn {sProductionStep.ProductionStepId} của sản phẩm {sProductionStep.ProductId}", req.JsonSerialize());
                    return true;
                }catch(Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductionStagesById");
                    throw;
                }
            }

        }

        private async Task<IList<InOutStepLink>> InsertAndUpdateProductInStep(int productionStepId, List<ProductInStepInfo> source)
        {
            var nProductionInSteps = source.Where(x => x.ProductInStepId <= 0)
                                            .Select(x => _mapper.Map<ProductInStep>((ProductInStepModel)x)).ToList();

            var uProductionInSteps = source.Where(x => x.ProductInStepId > 0)
                                            .Select(x => (ProductInStepModel)x).ToList();
            var destProductionInSteps = _manufacturingDBContext.ProductInStep.Where(x => uProductionInSteps.Select(y => y.ProductId).Contains(x.ProductId)).ToList();
            foreach (var d in destProductionInSteps)
            {
                var s = uProductionInSteps.FirstOrDefault(s => s.ProductId == d.ProductId);
                _mapper.Map(s, d);
            }

            await _manufacturingDBContext.ProductInStep.AddRangeAsync(nProductionInSteps);
            await _manufacturingDBContext.SaveChangesAsync();

            var inOutStepLinks = source.Where(x => x.ProductInStepId > 0).Select(x=> new InOutStepLink
            {
                InOutStepType = (int)x.InOutStepType,
                ProductInStepId = x.ProductInStepId,
                ProductionStepId = productionStepId
            }).ToList();

            foreach (var p in nProductionInSteps)
            {
                var s = source.FirstOrDefault(x => x.ProductId == p.ProductId);
                if (s == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                inOutStepLinks.Add(new InOutStepLink
                {
                    InOutStepType = (int)s.InOutStepType,
                    ProductInStepId = p.ProductInStepId,
                    ProductionStepId = productionStepId
                });
            }

            return inOutStepLinks;
        }
    }
}
