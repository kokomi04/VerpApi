﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.SemiConversion;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductSemi.Implement
{
    public class ProductSemiConversionService : IProductSemiConversionService
    {
        private readonly ManufacturingDBContext _manuDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductSemiConversionService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductSemiService> logger
            , IMapper mapper)
        {
            _manuDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductSemiConversion);
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<long> AddProductSemiConversion(ProductSemiConversionModel model)
        {
            var entity = _mapper.Map<ProductSemiConversion>(model);
            _manuDBContext.ProductSemiConversion.Add(entity);
            await _manuDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => ProductSemiConversionActivityLogMessage.Create)
                   .MessageResourceFormatDatas(entity.ProductSemi?.Title)
                   .ObjectId(entity.ProductSemiConversionId)
                   .JsonData(entity)
                   .CreateLog();

            return entity.ProductSemiConversionId;
        }

        public async Task<bool> DeleteProductSemiConversion(long productSemiId, long productSemiConversionId)
        {
            var entity = await _manuDBContext.ProductSemiConversion.FirstOrDefaultAsync(x => x.ProductSemiId == productSemiId && x.ProductSemiConversionId == productSemiConversionId);
            if (entity == null)
                throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemiConversion);

            entity.IsDeleted = true;
            await _manuDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => ProductSemiConversionActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(entity.ProductSemi?.Title)
                   .ObjectId(entity.ProductSemiConversionId)
                   .ObjectType(EnumObjectType.ProductSemiConversion)
                   .JsonData(entity)
                   .CreateLog();

            return true;
        }

        public async Task<ICollection<ProductSemiConversionModel>> GetAllProductSemiConversionsByProductSemi(long productSemiId)
        {
            var results = await _manuDBContext.ProductSemiConversion.Where(x => x.ProductSemiId == productSemiId)
                .ProjectTo<ProductSemiConversionModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return results;
        }

        public async Task<ProductSemiConversionModel> GetProductSemiConversion(long productSemiConversionId)
        {
            var entity = await _manuDBContext.ProductSemiConversion.FirstOrDefaultAsync(x => x.ProductSemiConversionId == productSemiConversionId);
            if (entity == null)
                throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemiConversion);
            return _mapper.Map<ProductSemiConversionModel>(entity);
        }

        public async Task<bool> UpdateProductSemiConversion(long productSemiConversionId, ProductSemiConversionModel model)
        {
            var entity = await _manuDBContext.ProductSemiConversion.FirstOrDefaultAsync(x => x.ProductSemiConversionId == productSemiConversionId);
            if (entity == null)
                throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemiConversion);

            _mapper.Map(model, entity);
            await _manuDBContext.SaveChangesAsync();

            await _objActivityLogFacade.LogBuilder(() => ProductSemiConversionActivityLogMessage.Update)
                   .MessageResourceFormatDatas(entity.ProductSemi?.Title)
                   .ObjectId(entity.ProductSemiConversionId)
                   .JsonData(entity)
                   .CreateLog();
            return true;
        }
    }
}
