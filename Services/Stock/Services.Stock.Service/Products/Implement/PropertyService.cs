﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Product;
using static Verp.Resources.Stock.Product.PropertyValidationMessage;

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
                 .JsonData(req)
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
                 .JsonData(propertyEntity)
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

        public async Task<IList<PropertyModel>> GetByIds(IList<int> propertyIds)
        {
            if (propertyIds == null || propertyIds.Count == 0)
                return new List<PropertyModel>();
            var properties = await _stockDbContext.Property.Where(p => propertyIds.Contains(p.PropertyId)).ToListAsync();

            return _mapper.Map<List<PropertyModel>>(properties);
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
                 .JsonData(req)
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
