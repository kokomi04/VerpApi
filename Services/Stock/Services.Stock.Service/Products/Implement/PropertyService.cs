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
    public class PropertyService : IPropertyService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public PropertyService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<PropertyService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<int> CreateProperty(PropertyModel req)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyName == req.PropertyName);
                if(propertyEntity != null) throw new BadRequestException(GeneralCode.InvalidParams, "Tên thuộc tính đã tồn tại");
                propertyEntity = _mapper.Map<Property>(req);

                await _stockDbContext.Property.AddAsync(propertyEntity);
                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductProperty, propertyEntity.PropertyId, $"Tạo mới thuộc tính sản phẩm {propertyEntity.PropertyId}", req.JsonSerialize());
                return propertyEntity.PropertyId;

            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("CreateProperty", ex);
                throw;
            }
        }

        public async Task<bool> DeleteProperty(int propertyId)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyId == propertyId);
                if (propertyEntity == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính sản phẩm không tồn tại");

                if(_stockDbContext.ProductProperty.Any(mp => mp.ProductPropertyId == propertyId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính sản phẩm đang được sử dụng trong BOM");

                propertyEntity.IsDeleted = true;
                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductSemi, propertyEntity.PropertyId, $"Xóa thuộc tính sản phẩm {propertyEntity.PropertyId}", propertyEntity.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("DeleteProperty", ex);
                throw;
            }
        }

        public async Task<IList<PropertyModel>> GetProperties()
        {
            var ls = await _stockDbContext.Property
                .ProjectTo<PropertyModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return ls;
        }

        public async Task<PropertyModel> GetProperty(int propertyId)
        {
            var property = await _stockDbContext.Property.Where(p => p.PropertyId == propertyId).FirstOrDefaultAsync();
            if (property == null) throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính sản phẩm không tồn tại");
            return _mapper.Map<PropertyModel>(property);
        }

        public async Task<int> UpdateProperty(int propertyId, PropertyModel req)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyName == req.PropertyName && p.PropertyId != propertyId);
                if (propertyEntity != null) throw new BadRequestException(GeneralCode.InvalidParams, "Tên thuộc sản phẩm tính đã tồn tại");
                propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyId == propertyId);
                if (propertyEntity == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thuộc tính không tồn tại");
                req.PropertyId = propertyId;
                _mapper.Map(req, propertyEntity);
                await _stockDbContext.SaveChangesAsync();
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductProperty, propertyEntity.PropertyId, $"Cập nhật thuộc tính sản phẩm {propertyEntity.PropertyId}", req.JsonSerialize());
                return propertyId;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("UpdateProperty", ex);
                throw;
            }
        }
    }
}
