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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductSemi;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductSemi.Implement
{
    public class ProductSemiService : IProductSemiService
    {
        private readonly ManufacturingDBContext _manuDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductSemiService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductSemiService> logger
            , IMapper mapper)
        {
            _manuDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<long> CreateProductSemi(ProductSemiModel model)
        {
            var trans = await _manuDBContext.Database.BeginTransactionAsync();
            try
            {
                var productSemiEntity = _mapper.Map<ProductSemiEntity>(model);
                await _manuDBContext.ProductSemi.AddAsync(productSemiEntity);
                await _manuDBContext.SaveChangesAsync();

                if (model.ProductSemiConversions.Count() > 0)
                {
                    foreach (var conversion in model.ProductSemiConversions)
                    {
                        conversion.ProductSemiId = productSemiEntity.ProductSemiId;
                    }

                    var lsConversionEntity = _mapper.Map<ICollection<ProductSemiConversion>>(model.ProductSemiConversions);
                    await _manuDBContext.ProductSemiConversion.AddRangeAsync(lsConversionEntity);
                    await _manuDBContext.SaveChangesAsync();
                }

                await trans.CommitAsync();
                await _activityLogService.CreateLog(Commons.Enums.MasterEnum.EnumObjectType.ProductSemi, productSemiEntity.ProductSemiId, $"Tạo mới bán thành phẩm {productSemiEntity.ProductSemiId}", model.JsonSerialize());
                return productSemiEntity.ProductSemiId;

            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("CreateProductSemi", ex);
                throw;
            }


        }

        public async Task<bool> DeleteProductSemi(long productSemiId)
        {
            var trans = await _manuDBContext.Database.BeginTransactionAsync();
            try
            {
                var productSemiEntity = await _manuDBContext.ProductSemi.FirstOrDefaultAsync(p => p.ProductSemiId == productSemiId);
                if (productSemiEntity == null)
                    throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);
                var productSemiConversions = await _manuDBContext.ProductSemiConversion.Where(p => p.ProductSemiId == productSemiId).ToListAsync();

                productSemiEntity.IsDeleted = true;
                productSemiConversions.ForEach(x => x.IsDeleted = true);
                await _manuDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(Commons.Enums.MasterEnum.EnumObjectType.ProductSemi, productSemiEntity.ProductSemiId, $"Xóa bán thành phẩm {productSemiEntity.ProductSemiId}", productSemiEntity.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("DeleteProductSemi", ex);
                throw;
            }

        }

        public async Task<IList<ProductSemiModel>> GetListProductSemi(long containerId, int containerTypeId)
        {
            var ls = await _manuDBContext.ProductSemi.Where(x => x.ContainerId == containerId && x.ContainerTypeId == containerTypeId)
                .ProjectTo<ProductSemiModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return ls;
        }

        public async Task<IList<ProductSemiModel>> GetListProductSemiListProductSemiId(IList<long> productSemiIds)
        {
            var ls = await _manuDBContext.ProductSemi.Where(x => productSemiIds.Contains(x.ProductSemiId))
                 .ProjectTo<ProductSemiModel>(_mapper.ConfigurationProvider)
                 .ToListAsync();
            return ls;
        }

        public async Task<ProductSemiModel> GetListProductSemiById(long productSemiId)
        {
            var data = await _manuDBContext.ProductSemi.ProjectTo<ProductSemiModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(p => p.ProductSemiId == productSemiId);
            if (data == null)
                throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);
            return data;
        }

        public async Task<IList<ProductSemiModel>> GetListProductSemiByListContainerId(IList<long> lsContainerId)
        {
            return await _manuDBContext.ProductSemi.ProjectTo<ProductSemiModel>(_mapper.ConfigurationProvider).Where(p => lsContainerId.Contains(p.ContainerId)).ToListAsync();
        }

        public async Task<bool> UpdateProductSemi(long productSemiId, ProductSemiModel model)
        {
            var trans = await _manuDBContext.Database.BeginTransactionAsync();
            try
            {
                var productSemiEntity = await _manuDBContext.ProductSemi.FirstOrDefaultAsync(p => p.ProductSemiId == productSemiId);
                if (productSemiEntity == null)
                    throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);

                if (model.ProductSemiConversions.Count() > 0)
                {
                    var productSemiConversions = await _manuDBContext.ProductSemiConversion.Where(p => p.ProductSemiId == productSemiId).ToListAsync();
                    foreach (var conversion in productSemiConversions)
                    {
                        var modify = model.ProductSemiConversions.FirstOrDefault(x => x.ProductSemiConversionId == conversion.ProductSemiConversionId);
                        if (modify != null)
                            _mapper.Map(modify, conversion);
                        else conversion.IsDeleted = true;
                    }

                    var newConventionEntity = _mapper.Map<IList<ProductSemiConversion>>(model.ProductSemiConversions.Where(x => x.ProductSemiConversionId == 0));
                    await _manuDBContext.ProductSemiConversion.AddRangeAsync(newConventionEntity);
                }

                _mapper.Map(model, productSemiEntity);
                await _manuDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(Commons.Enums.MasterEnum.EnumObjectType.ProductSemi, productSemiEntity.ProductSemiId, $"Cập nhật bán thành phẩm {productSemiEntity.ProductSemiId}", model.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("UpdateProductSemi", ex);
                throw;
            }
        }
    }
}
