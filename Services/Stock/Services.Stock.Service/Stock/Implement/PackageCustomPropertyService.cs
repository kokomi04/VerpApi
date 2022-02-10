using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Stock.Package;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class PackageCustomPropertyService : IPackageCustomPropertyService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ObjectActivityLogFacade _packageCustomPropertyActivityLog;
        private readonly IMapper _mapper;


        public PackageCustomPropertyService(StockDBContext stockContext
           , ILogger<PackageService> logger
           , IActivityLogService activityLogService
            , IMapper mapper
           )
        {
            _stockDbContext = stockContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _packageCustomPropertyActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PackageCustomProperty);
            _mapper = mapper;
        }

        public async Task<int> Create(PackageCustomPropertyModel model)
        {
            var info = _mapper.Map<PackageCustomProperty>(model);
            await _stockDbContext.PackageCustomProperty.AddAsync(info);
            await _stockDbContext.SaveChangesAsync();


            await _packageCustomPropertyActivityLog.LogBuilder(() => PackageCustomPropertyActivityLogMessage.Create)
              .MessageResourceFormatDatas(model.Title)
              .ObjectId(info.PackageCustomPropertyId)
              .JsonData(model.JsonSerialize())
              .CreateLog();

            return info.PackageCustomPropertyId;
        }

        public async Task<bool> Delete(int packageCustomPropertyId)
        {
            var info = await _stockDbContext.PackageCustomProperty.FirstOrDefaultAsync(p => p.PackageCustomPropertyId == packageCustomPropertyId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            info.IsDeleted = true;

            await _stockDbContext.SaveChangesAsync();


            await _packageCustomPropertyActivityLog.LogBuilder(() => PackageCustomPropertyActivityLogMessage.Delete)
              .MessageResourceFormatDatas(info.Title)
              .ObjectId(info.PackageCustomPropertyId)
              .JsonData(info.JsonSerialize())
              .CreateLog();

            return true;
        }

        public async Task<IList<PackageCustomPropertyModel>> Get()
        {
            var lst = await _stockDbContext.PackageCustomProperty.ToListAsync();

            return _mapper.Map<List<PackageCustomPropertyModel>>(lst);
        }

        public async Task<PackageCustomPropertyModel> Info(int packageCustomPropertyId)
        {
            var info = await _stockDbContext.PackageCustomProperty.FirstOrDefaultAsync(p => p.PackageCustomPropertyId == packageCustomPropertyId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            return _mapper.Map<PackageCustomPropertyModel>(info);
        }

        public async Task<bool> Update(int packageCustomPropertyId, PackageCustomPropertyModel model)
        {
            var info = await _stockDbContext.PackageCustomProperty.FirstOrDefaultAsync(p => p.PackageCustomPropertyId == packageCustomPropertyId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            model.PackageCustomPropertyId = info.PackageCustomPropertyId;
            _mapper.Map(model, info);
            info.IsDeleted = true;

            await _stockDbContext.SaveChangesAsync();

            await _packageCustomPropertyActivityLog.LogBuilder(() => PackageCustomPropertyActivityLogMessage.Delete)
              .MessageResourceFormatDatas(info.Title)
              .ObjectId(info.PackageCustomPropertyId)
              .JsonData(info.JsonSerialize())
              .CreateLog();

            return true;
        }
    }
}
