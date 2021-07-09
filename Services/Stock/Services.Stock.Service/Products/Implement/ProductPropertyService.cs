using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using Microsoft.Data.SqlClient;
using VErp.Infrastructure.EF.EFExtensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Data;
using System.IO;
using VErp.Commons.Library.Model;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Service.Products.Implement.ProductBomFacade;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductPropertyService : IProductPropertyService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        private readonly IUnitService _unitService;
        private readonly IProductService _productService;
        private readonly IManufacturingHelperService _manufacturingHelperService;

        public ProductPropertyService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductPropertyService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IUnitService unitService
            , IProductService productService
            , IManufacturingHelperService manufacturingHelperService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _unitService = unitService;
            _productService = productService;
            _manufacturingHelperService = manufacturingHelperService;
        }

        public async Task<int> CreateProductProperty(ProductPropertyModel req)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var productPropertyEntity = await _stockDbContext.ProductProperty.FirstOrDefaultAsync(p => p.PropertyName == req.ProductPropertyName);
                if(productPropertyEntity != null) throw new BadRequestException(GeneralCode.InvalidParams, "Tên thuộc tính đã tồn tại");
                productPropertyEntity = _mapper.Map<ProductProperty>(req);

                await _stockDbContext.ProductProperty.AddAsync(productPropertyEntity);
                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductProperty, productPropertyEntity.ProductPropertyId, $"Tạo mới thuộc tính sản phẩm {productPropertyEntity.ProductMaterialProperty}", req.JsonSerialize());
                return productPropertyEntity.ProductPropertyId;

            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("CreateProductProperty", ex);
                throw;
            }
        }

        public async Task<bool> DeleteProductProperty(int productPropertyId)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var productPropertyEntity = await _stockDbContext.ProductProperty.FirstOrDefaultAsync(p => p.ProductPropertyId == productPropertyId);
                if (productPropertyEntity == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính không tồn tại");

                if(_stockDbContext.ProductMaterialProperty.Any(mp => mp.ProductPropertyId == productPropertyId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính đang được sử dụng trong BOM");

                productPropertyEntity.IsDeleted = true;
                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(Commons.Enums.MasterEnum.EnumObjectType.ProductSemi, productPropertyEntity.ProductPropertyId, $"Xóa thuộc tính {productPropertyEntity.ProductPropertyId}", productPropertyEntity.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("DeleteProductProperty", ex);
                throw;
            }
        }

        public async Task<IList<ProductPropertyModel>> GetProductProperties()
        {
            var ls = await _stockDbContext.ProductProperty
                .ProjectTo<ProductPropertyModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return ls;
        }

        public async Task<ProductPropertyModel> GetProductProperty(int productPropertyId)
        {
            var productProperty = await _stockDbContext.ProductProperty.Where(p => p.ProductPropertyId == productPropertyId).FirstOrDefaultAsync();
            if (productProperty == null) throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính không tồn tại");
            return _mapper.Map<ProductPropertyModel>(productProperty);
        }

        public async Task<int> UpdateProductProperty(int productPropertyId, ProductPropertyModel req)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var productPropertyEntity = await _stockDbContext.ProductProperty.FirstOrDefaultAsync(p => p.PropertyName == req.ProductPropertyName && p.ProductPropertyId != productPropertyId);
                if (productPropertyEntity != null) throw new BadRequestException(GeneralCode.InvalidParams, "Tên thuộc tính đã tồn tại");
                productPropertyEntity = await _stockDbContext.ProductProperty.FirstOrDefaultAsync(p => p.ProductPropertyId == productPropertyId);
                if (productPropertyEntity == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính không tồn tại");
                req.ProductPropertyId = productPropertyId;
                _mapper.Map(req, productPropertyEntity);
                await _stockDbContext.SaveChangesAsync();
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductProperty, productPropertyEntity.ProductPropertyId, $"Cập nhật thuộc tính {productPropertyEntity.ProductPropertyId}", req.JsonSerialize());
                return productPropertyId;
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
