﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Print;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Input;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Report;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Voucher;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;
using ObjectPrintConfigMappingEntity = VErp.Infrastructure.EF.MasterDB.ObjectPrintConfigMapping;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ObjectPrintConfigService : IObjectPrintConfigService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        private readonly IInputPrivateTypeHelperService _inputPrivateTypeHelperService;
        private readonly IInputPublicTypeHelperService _inputPublicTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IReportTypeHelperService _reportTypeHelperService;

        private readonly ICurrentContextService _currentContextService;

        private readonly ObjectActivityLogFacade _objectPrintConfigActivityLog;

        public ObjectPrintConfigService(MasterDBContext masterDbContext
            , ILogger<ObjectGenCodeService> logger
            , IInputPrivateTypeHelperService inputPrivateTypeHelperService
            , IInputPublicTypeHelperService inputPublicTypeHelperService
            , IVoucherTypeHelperService voucherTypeHelperService
            , IMapper mapper
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService, IOrganizationHelperService organizationHelperService, IReportTypeHelperService reportTypeHelperService)
        {
            _masterDbContext = masterDbContext;
            _mapper = mapper;
            _logger = logger;
            _inputPrivateTypeHelperService = inputPrivateTypeHelperService;
            _inputPublicTypeHelperService = inputPublicTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
            _currentContextService = currentContextService;
            _objectPrintConfigActivityLog = activityLogService.CreateObjectTypeActivityLog(null);
            _organizationHelperService = organizationHelperService;
            _reportTypeHelperService = reportTypeHelperService;
        }

        public async Task<ObjectPrintConfig> GetObjectPrintConfigMapping(EnumObjectType objectTypeId, int objectId)
        {
            var maps = await _masterDbContext.ObjectPrintConfigMapping.Where(x => x.ObjectTypeId == (int)objectTypeId && x.ObjectId == objectId).ToArrayAsync();

            return new ObjectPrintConfig
            {
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                PrintConfigIds = maps.Select(x => x.PrintConfigCustomId).ToArray()
            };
        }

        public async Task<bool> MapObjectPrintConfig(ObjectPrintConfig mapping)
        {
            var trans = await _masterDbContext.Database.BeginTransactionAsync();
            try
            {
                await MapObjectPrintConfigCustom(mapping);
                await MapObjectPrintConfigStandard(mapping);

                await trans.CommitAsync();


                await _objectPrintConfigActivityLog.LogBuilder(() => ObjectPrintConfigActivityLogMessage.MapingObjectPrintConfigs)
                 .ObjectType(mapping.ObjectTypeId)
                 .ObjectId(mapping.ObjectId)
                 .JsonData(mapping)
                 .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "MapObjectPrintConfig");
                throw;
            }


        }

        private async Task MapObjectPrintConfigCustom(ObjectPrintConfig mapping)
        {
            var mappingModels = mapping.PrintConfigIds
                .Select(printConfigId => new ObjectPrintConfigMappingModel
                {
                    ObjectId = mapping.ObjectId,
                    ObjectTypeId = mapping.ObjectTypeId,
                    PrintConfigId = printConfigId
                })
                .AsQueryable()
                .ProjectTo<ObjectPrintConfigMappingEntity>(_mapper.ConfigurationProvider)
                .ToArray();

            var oldObjectPrintConfigs = await _masterDbContext.ObjectPrintConfigMapping
                .Where(x => x.ObjectTypeId == (int)mapping.ObjectTypeId && x.ObjectId == mapping.ObjectId)
                .ToArrayAsync();
            _masterDbContext.ObjectPrintConfigMapping.RemoveRange(oldObjectPrintConfigs);
            await _masterDbContext.SaveChangesAsync();

            await _masterDbContext.ObjectPrintConfigMapping.AddRangeAsync(mappingModels);
            await _masterDbContext.SaveChangesAsync();

        }

        private async Task MapObjectPrintConfigStandard(ObjectPrintConfig mapping)
        {
            if (_currentContextService.IsDeveloper)
            {
                var printConfigStandardIdMap = (await _masterDbContext.PrintConfigCustom.AsNoTracking()
                   .Where(x => mapping.PrintConfigIds.Contains(x.PrintConfigCustomId) && x.PrintConfigStandardId.HasValue && x.PrintConfigStandardId.Value > 0)
                   .ToListAsync())
                   .ToDictionary(k => k.PrintConfigCustomId, v => v.PrintConfigStandardId);

                var mappingModels = mapping.PrintConfigIds
                .Where(printConfigId => printConfigStandardIdMap.Keys.Contains(printConfigId))
                .Select(printConfigId => new ObjectPrintConfigStandardMapping
                {
                    ObjectId = mapping.ObjectId,
                    ObjectTypeId = (int)mapping.ObjectTypeId,
                    PrintConfigStandardId = (int)printConfigStandardIdMap[printConfigId]
                })
                .GroupBy(x => x.PrintConfigStandardId)
                .Select(x => x.FirstOrDefault())
                .ToArray();

                var oldObjectPrintConfigs = await _masterDbContext.ObjectPrintConfigStandardMapping
                    .Where(x => x.ObjectTypeId == (int)mapping.ObjectTypeId && x.ObjectId == mapping.ObjectId)
                    .ToArrayAsync();
                _masterDbContext.ObjectPrintConfigStandardMapping.RemoveRange(oldObjectPrintConfigs);
                await _masterDbContext.SaveChangesAsync();

                await _masterDbContext.ObjectPrintConfigStandardMapping.AddRangeAsync(mappingModels);
                await _masterDbContext.SaveChangesAsync();
            }
        }

        private IList<ObjectPrintConfigMappingEntity> _objectPrintConfigMappings;
        private IList<PrintConfigCustom> _printConfigs;
        public async Task<PageData<ObjectPrintConfigSearch>> GetObjectPrintConfigSearch(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim().ToLower();

            _objectPrintConfigMappings = await _masterDbContext.ObjectPrintConfigMapping.AsNoTracking().ToListAsync();
            _printConfigs = await _masterDbContext.PrintConfigCustom.AsNoTracking().ToListAsync();

            var result = new List<ObjectPrintConfigSearch>();

            var poTask = PurchaseOrderMappingTypeModels();
            var vourcherTask = VourcherMappingTypeModels();
            var hrTask = HrMappingTypeModels();
            var inputPrivateTask = InputPrivateMappingTypeModels();
            var inputPublicTask = InputPublicMappingTypeModels();
            var manufactureTask = ManufactureMappingTypeModels();
            var reportTypesTask = ReportMappingTypeModels();

            result.AddRange(poTask);
            result.AddRange(await vourcherTask);
            result.AddRange(await inputPrivateTask);
            result.AddRange(await inputPublicTask);
            result.AddRange(await hrTask);
            result.AddRange(await reportTypesTask);
            result.AddRange(manufactureTask);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(c =>
                 c.ObjectTypeName?.ToLower().Contains(keyword) == true
                 || c.ObjectTitle?.ToLower().Contains(keyword) == true
                ).ToList();
            }
            var total = result.Count;

            if (size > 0)
            {
                result = result.Skip((page - 1) * size).Take(size).ToList();
            }
            return (result, total);
        }

        private IList<ObjectPrintConfigSearch> PurchaseOrderMappingTypeModels()
        {
            IList<ObjectPrintConfigSearch> result = new List<ObjectPrintConfigSearch>();

            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.PurchaseOrder,
                objectTypeId: EnumObjectType.PurchasingRequest,
                objectId: 0,
                objectTitle: "Yêu cầu vật tư")
            );

            result.Add(
               GetObjectPrintConfigSearch(
               moduleTypeId: EnumModuleType.PurchaseOrder,
               objectTypeId: EnumObjectType.PurchasingSuggest,
               objectId: 0,
               objectTitle: "Đề nghị mua hàng")
           );


            result.Add(
              GetObjectPrintConfigSearch(
              moduleTypeId: EnumModuleType.PurchaseOrder,
              objectTypeId: EnumObjectType.PurchaseOrder,
              objectId: 0,
              objectTitle: "Đơn đặt mua")
          );


            return result;
        }

        private IList<ObjectPrintConfigSearch> ManufactureMappingTypeModels()
        {
            IList<ObjectPrintConfigSearch> result = new List<ObjectPrintConfigSearch>();

            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.Manufacturing,
                objectTypeId: EnumObjectType.ProductionAssignment,
                objectId: 1,
                objectTitle: "Phân công sản xuất")
            );

            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.Manufacturing,
                objectTypeId: EnumObjectType.ProductionAssignment,
                objectId: 2,
                objectTitle: "Phiếu phân công sản xuất")
            );

            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.Manufacturing,
                objectTypeId: EnumObjectType.ProductionSchedule,
                objectTitle: "Kế hoạch sản xuất")
            );

            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.Manufacturing,
                objectTypeId: EnumObjectType.ProductionOrder,
                objectTitle: "Lệch sản xuất")
            );

            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.Manufacturing,
                objectTypeId: EnumObjectType.ProductionHandover,
                objectTitle: "Bàn giao sản xuất")
            );

            return result;
        }

        private async Task<IList<ObjectPrintConfigSearch>> VourcherMappingTypeModels()
        {
            var voucherTypes = _voucherTypeHelperService.GetVoucherTypeSimpleList();

            var result = new List<ObjectPrintConfigSearch>();
            foreach (var voucherType in await voucherTypes)
            {
                result.Add(
                         GetObjectPrintConfigSearch(
                         moduleTypeId: EnumModuleType.PurchaseOrder,
                         objectTypeId: EnumObjectType.VoucherType,
                         objectId: voucherType.VoucherTypeId,
                         objectTitle: voucherType.Title
                         )
                     );
            }

            return result;
        }

        private async Task<IList<ObjectPrintConfigSearch>> HrMappingTypeModels()
        {
            var hrTypes = _organizationHelperService.GetHrTypeSimpleList();

            var result = new List<ObjectPrintConfigSearch>();
            foreach (var hrType in await hrTypes)
            {
                result.Add(
                         GetObjectPrintConfigSearch(
                         moduleTypeId: EnumModuleType.Organization,
                         objectTypeId: EnumObjectType.HrType,
                         objectId: hrType.HrTypeId,
                         objectTitle: hrType.Title
                         )
                     );
            }


            result.Add(
                GetObjectPrintConfigSearch(
                moduleTypeId: EnumModuleType.Organization,
                objectTypeId: EnumObjectType.SalaryEmployee,
                objectTitle: "Bảng lương nhân sự")
            );

            return result;
        }

        private async Task<IList<ObjectPrintConfigSearch>> InputPrivateMappingTypeModels()
        {
            var inputTypes = _inputPrivateTypeHelperService.GetInputTypeSimpleList();

            var result = new List<ObjectPrintConfigSearch>();
            foreach (var inputType in await inputTypes)
            {
                result.Add(
                        GetObjectPrintConfigSearch(
                        moduleTypeId: EnumModuleType.Accountant,
                        objectTypeId: EnumObjectType.InputType,
                        objectId: inputType.InputTypeId,
                        objectTitle: inputType.Title
                        )
                    );
            }

            return result;
        }


        private async Task<IList<ObjectPrintConfigSearch>> InputPublicMappingTypeModels()
        {
            var inputTypes = _inputPublicTypeHelperService.GetInputTypeSimpleList();

            var result = new List<ObjectPrintConfigSearch>();
            foreach (var inputType in await inputTypes)
            {
                result.Add(
                        GetObjectPrintConfigSearch(
                        moduleTypeId: EnumModuleType.AccountantPublic,
                        objectTypeId: EnumObjectType.InputTypePublic,
                        objectId: inputType.InputTypeId,
                        objectTitle: inputType.Title
                        )
                    );
            }

            return result;
        }


        private async Task<IList<ObjectPrintConfigSearch>> ReportMappingTypeModels()
        {
            var groups = await _reportTypeHelperService.GetGroups();

            var reportTypes = _reportTypeHelperService.GetReportTypeSimpleList();

            var result = new List<ObjectPrintConfigSearch>();

            result.Add(
                       GetObjectPrintConfigSearch(
                       moduleTypeId: EnumModuleType.Report,
                       objectTypeId: EnumObjectType.ReportType,
                       objectId: 0,
                       objectTitle: "Mặc định"
                       )
                   );

            foreach (var reportType in (await reportTypes).List)
            {
                var groupInfo = groups.FirstOrDefault(g => g.ReportTypeGroupId == reportType.ReportTypeGroupId);
                var title = reportType.ReportTypeName;
                if (groupInfo != null)
                    title = groupInfo.ReportTypeGroupName + " " + title;
                result.Add(
                        GetObjectPrintConfigSearch(
                        moduleTypeId: EnumModuleType.Report,
                        objectTypeId: EnumObjectType.ReportType,
                        objectId: reportType.ReportTypeId ?? 0,
                        objectTitle: title
                        )
                    );
            }



            return result;
        }

        private ObjectPrintConfigSearch GetObjectPrintConfigSearch(
            EnumModuleType moduleTypeId,
            EnumObjectType objectTypeId, int objectId = 0, string objectTitle = "")
        {
            var mapping = _objectPrintConfigMappings.Where(m => m.ObjectTypeId == (int)objectTypeId && m.ObjectId == objectId).ToList();
            var printConfig = _printConfigs.Where(c => mapping?.Select(x => x.PrintConfigCustomId).Contains(c.PrintConfigCustomId) == true).Select(x => x.PrintConfigCustomId).ToArray();

            return new ObjectPrintConfigSearch()
            {
                PrintConfigIds = printConfig,
                ObjectId = objectId,
                ObjectTitle = objectTitle,
                ObjectTypeId = objectTypeId,
                ObjectTypeName = objectTypeId.GetEnumDescription(),
                ModuleTypeId = moduleTypeId,
                ModuleTypeName = moduleTypeId.GetEnumDescription()
            };
        }
    }
}
