using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
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
        private readonly IActivityLogService _activityLogService;

        private readonly IInputTypeHelperService _inputTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;

        private readonly ICurrentContextService _currentContextService;

        public ObjectPrintConfigService(MasterDBContext masterDbContext
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService
            , IInputTypeHelperService inputTypeHelperService
            , IVoucherTypeHelperService voucherTypeHelperService
            , IMapper mapper
            , ICurrentContextService currentContextService)
        {
            _masterDbContext = masterDbContext;
            _mapper = mapper;
            _logger = logger;
            _activityLogService = activityLogService;
            _inputTypeHelperService = inputTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
            _currentContextService = currentContextService;
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
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "MapObjectPrintConfig");
                throw;
            }
            

        }

        public async Task MapObjectPrintConfigCustom(ObjectPrintConfig mapping)
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
                .Where(x => x.ObjectTypeId == (int)mapping.ObjectTypeId && x.ObjectId == mapping.ObjectId && mapping.PrintConfigIds.Contains(x.PrintConfigCustomId))
                .ToArrayAsync();
            _masterDbContext.ObjectPrintConfigMapping.RemoveRange(oldObjectPrintConfigs);
            await _masterDbContext.SaveChangesAsync();

            await _masterDbContext.ObjectPrintConfigMapping.AddRangeAsync(mappingModels);
            await _masterDbContext.SaveChangesAsync();

        }

        public async Task MapObjectPrintConfigStandard(ObjectPrintConfig mapping)
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
            keyword = keyword?.ToLower();
            _objectPrintConfigMappings = await _masterDbContext.ObjectPrintConfigMapping.AsNoTracking().ToListAsync();
            _printConfigs = await _masterDbContext.PrintConfigCustom.AsNoTracking().ToListAsync();

            var result = new List<ObjectPrintConfigSearch>();


            var vourcherTask = VourcherMappingTypeModels();
            var inputTask = InputMappingTypeModels();
            var manufactureTask = ManufactureMappingTypeModels();

            result.AddRange(await vourcherTask);
            result.AddRange(await inputTask);
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

        private async Task<IList<ObjectPrintConfigSearch>> InputMappingTypeModels()
        {
            var inputTypes = _inputTypeHelperService.GetInputTypeSimpleList();

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
