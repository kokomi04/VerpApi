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
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Services.Stock.Service.Resources.Product;
using static VErp.Services.Stock.Service.Resources.Product.PropertyValidationMessage;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class PropertyService : IPropertyService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _propertyActivityLog;

        public PropertyService(StockDBContext stockContext
            , ILogger<PropertyService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _logger = logger;
            _mapper = mapper;
            _propertyActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Property);
        }

        public async Task<int> CreateProperty(PropertyModel req)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                await Validate(null, req);

                var propertyEntity = _mapper.Map<Property>(req);

                await _stockDbContext.Property.AddAsync(propertyEntity);
                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _propertyActivityLog.LogBuilder(() => PropertyActivityLogMessage.Create)
                 .MessageResourceFormatDatas(req.PropertyCode)
                 .ObjectId(propertyEntity.PropertyId)
                 .JsonData(req.JsonSerialize())
                 .CreateLog();

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
                    throw PropertyNotFound.BadRequest();

                if (_stockDbContext.ProductProperty.Any(mp => mp.ProductPropertyId == propertyId))
                    throw CanNotDeletePropertyInUsed.BadRequest();

                propertyEntity.IsDeleted = true;
                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _propertyActivityLog.LogBuilder(() => PropertyActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(propertyEntity.PropertyCode)
                 .ObjectId(propertyEntity.PropertyId)
                 .JsonData(propertyEntity.JsonSerialize())
                 .CreateLog();
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
            if (property == null) throw PropertyNotFound.BadRequest();
            return _mapper.Map<PropertyModel>(property);
        }

        public async Task<int> UpdateProperty(int propertyId, PropertyModel req)
        {
            var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                await Validate(propertyId, req);

                var propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyId == propertyId);
                if (propertyEntity == null)
                    throw PropertyNotFound.BadRequest();
                req.PropertyId = propertyId;
                _mapper.Map(req, propertyEntity);
                await _stockDbContext.SaveChangesAsync();
                await trans.CommitAsync();

                await _propertyActivityLog.LogBuilder(() => PropertyActivityLogMessage.Update)
                 .MessageResourceFormatDatas(propertyEntity.PropertyCode)
                 .ObjectId(propertyEntity.PropertyId)
                 .JsonData(req.JsonSerialize())
                 .CreateLog();

                return propertyId;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("UpdateProperty", ex);
                throw;
            }
        }

        private async Task Validate(int? propertyId, PropertyModel req)
        {
            var propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyName == req.PropertyName && p.PropertyId != propertyId);
            if (propertyEntity != null) throw NameAlreadyExisted.BadRequestFormat(req.PropertyName);

            propertyEntity = await _stockDbContext.Property.FirstOrDefaultAsync(p => p.PropertyCode == req.PropertyCode && p.PropertyId != propertyId);
            if (propertyEntity != null) throw CodeAlreadyExisted.BadRequestFormat(req.PropertyCode);

        }
    }
}
