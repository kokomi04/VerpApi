using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.PlanExtraInfo;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{
    public class ProductionPlanExtraInfoService : IProductionPlanExtraInfoService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public ProductionPlanExtraInfoService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionPlanExtraInfoService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionPlanExtraInfo);
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<IList<ProductionPlanExtraInfoModel>> UpdateProductionPlanExtraInfo(int monthPlanId, IList<ProductionPlanExtraInfoModel> data)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var currentData = await _manufacturingDBContext.ProductionPlanExtraInfo
                    .Where(d => d.MonthPlanId == monthPlanId).ToListAsync();
                _manufacturingDBContext.ProductionPlanExtraInfo.RemoveRange(currentData);

                foreach (var item in data)
                {
                    var entity = _mapper.Map<ProductionPlanExtraInfo>(item);
                    entity.MonthPlanId = monthPlanId;
                    _manufacturingDBContext.ProductionPlanExtraInfo.Add(entity);
                }

                _manufacturingDBContext.SaveChanges();
                trans.Commit();

                await _objActivityLogFacade.LogBuilder(() => ProductionPlanExtraInfoActivityLogMessage.Update)
                   .MessageResourceFormatDatas(data?.Count)
                   .ObjectId(monthPlanId)
                   .JsonData(data)
                   .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateProductionPlanExtraInfo");
                throw;
            }
        }

        public async Task<IList<ProductionPlanExtraInfoModel>> GetProductionPlanExtraInfo(int monthPlanId)
        {
            var result = await _manufacturingDBContext.ProductionPlanExtraInfo
                .Where(d => d.MonthPlanId == monthPlanId)
                .ProjectTo<ProductionPlanExtraInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return result;
        }

    }
}
