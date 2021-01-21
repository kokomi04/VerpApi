using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Activity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using Verp.Cache.RedisCache;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ObjectGenCodeService : IObjectGenCodeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IGenCodeConfigService _genCodeConfigService;
        private readonly ICurrentContextService _currentContextService;

        private readonly IProductHelperService _productHelperService;
        private readonly IStockHelperService _stockHelperService;
        private readonly IInputTypeHelperService _inputTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;

        public ObjectGenCodeService(MasterDBContext masterDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService
            , IGenCodeConfigService genCodeConfigService
            , ICurrentContextService currentContextService
            , IProductHelperService productHelperService
            , IStockHelperService stockHelperService
            , IInputTypeHelperService inputTypeHelperService
            , IVoucherTypeHelperService voucherTypeHelperService
        )
        {
            _masterDbContext = masterDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _genCodeConfigService = genCodeConfigService;
            _currentContextService = currentContextService;
            _productHelperService = productHelperService;
            _stockHelperService = stockHelperService;
            _inputTypeHelperService = inputTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
        }


        public async Task<CustomGenCodeOutputModel> GetCurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId, long? fId, string code, long? date)
        {
            var customGenCodeId = (await _masterDbContext.ObjectCustomGenCodeMapping.FirstOrDefaultAsync(m => m.TargetObjectTypeId == (int)targetObjectTypeId && m.ConfigObjectTypeId == (int)configObjectTypeId && m.ConfigObjectId == configObjectId))?.CustomGenCodeId;

            CustomGenCode obj = null;

            if (customGenCodeId.HasValue)
            {
                obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(c => c.IsActived && c.CustomGenCodeId == customGenCodeId);
            }
            else
            {
                obj = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(c => c.IsActived && c.IsDefault);
            }


            if (obj == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotExisted, $"Chưa thiết định cấu hình sinh mã cho {targetObjectTypeId.GetEnumDescription()} {(configObjectId > 0 ? (long?)configObjectId : null)}");
            }

            return await _genCodeConfigService.GetInfo(obj.CustomGenCodeId, fId, code,date);
        }

        //public PageData<ObjectType> GetAllObjectType()
        //{
        //    var allData = EnumExtensions.GetEnumMembers<EnumObjectType>()
        //        .Where(m => m.Attributes != null && m.Attributes.Any(a => a.GetType() == typeof(GenCodeObjectAttribute)))
        //        .Select(m => new ObjectType
        //        {
        //            ObjectTypeId = m.Enum,
        //            ObjectTypeName = m.Description ?? m.Name.ToString()
        //        }).ToList();


        //    return (allData, allData.Count);
        //}


        public async Task<bool> MapObjectGenCode(ObjectGenCodeMapping model)
        {
            return await MapObjectCustomGenCode(new ObjectCustomGenCodeMapping
            {
                CustomGenCodeId = model.CustomGenCodeId,
                ObjectCustomGenCodeMappingId = model.ObjectCustomGenCodeMappingId,
                TargetObjectTypeId = (int)model.TargetObjectTypeId,
                ConfigObjectTypeId = (int)model.ConfigObjectTypeId,
                ObjectId = (int)model.ConfigObjectId,
                ConfigObjectId = model.ConfigObjectId,//default
                ObjectTypeId = (int)model.ObjectTypeId,
                UpdatedByUserId = _currentContextService.UserId
            });

        }

        public async Task<bool> MapObjectCustomGenCode(ObjectCustomGenCodeMapping model)
        {

            var config = await _masterDbContext.CustomGenCode.FirstOrDefaultAsync(c => c.CustomGenCodeId == model.CustomGenCodeId);
            if (config == null)
            {
                throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
            }
            var obj = await _masterDbContext.ObjectCustomGenCodeMapping.FirstOrDefaultAsync(m => m.TargetObjectTypeId == model.TargetObjectTypeId && m.ConfigObjectTypeId == model.ConfigObjectTypeId && m.ConfigObjectId == model.ConfigObjectId);
            if (obj == null)
            {
                _masterDbContext.ObjectCustomGenCodeMapping.Add(model);
                obj = model;
            }
            else
            {
                obj.CustomGenCodeId = model.CustomGenCodeId;
                obj.UpdatedByUserId = _currentContextService.UserId;
            }
            await _masterDbContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ObjectCustomGenCodeMapping, obj.ObjectCustomGenCodeMappingId, $"Gán sinh tùy chọn {config.CustomGenCodeName}", model.JsonSerialize());

            return true;

        }
        public async Task<bool> UpdateMultiConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, Dictionary<long, int> objectCustomGenCodes)
        {

            var dic = new Dictionary<ObjectCustomGenCodeMapping, CustomGenCode>();

            foreach (var mapConfig in objectCustomGenCodes)
            {
                var config = await _masterDbContext.CustomGenCode
                    .Where(c => c.IsActived)
                    .Where(c => c.CustomGenCodeId == mapConfig.Value)
                    .FirstOrDefaultAsync();
                if (config == null)
                {
                    throw new BadRequestException(CustomGenCodeErrorCode.CustomConfigNotFound);
                }
                var curMapConfig = await _masterDbContext.ObjectCustomGenCodeMapping
                    .FirstOrDefaultAsync(m => m.TargetObjectTypeId == (int)targetObjectTypeId && m.ConfigObjectTypeId == (int)configObjectTypeId && m.ConfigObjectId == mapConfig.Key);

                if (curMapConfig == null)
                {
                    curMapConfig = new ObjectCustomGenCodeMapping
                    {
                        CustomGenCodeId = mapConfig.Value,
                        TargetObjectTypeId = (int)targetObjectTypeId,
                        ConfigObjectTypeId = (int)configObjectTypeId,
                        ObjectId = (int)mapConfig.Key,
                        ConfigObjectId = mapConfig.Key,
                        ObjectTypeId = (int)targetObjectTypeId,
                        UpdatedByUserId = _currentContextService.UserId
                    };
                    _masterDbContext.ObjectCustomGenCodeMapping.Add(curMapConfig);
                }
                else if (curMapConfig.CustomGenCodeId != mapConfig.Value)
                {
                    curMapConfig.CustomGenCodeId = mapConfig.Value;
                }

                if (!dic.ContainsKey(curMapConfig))
                {
                    dic.Add(curMapConfig, config);
                }
            }
            await _masterDbContext.SaveChangesAsync();

            foreach (var item in dic)
            {
                await _activityLogService.CreateLog(EnumObjectType.ObjectCustomGenCodeMapping, item.Key.ObjectCustomGenCodeMappingId, $"Gán sinh tùy chọn (multi) {item.Value.CustomGenCodeName}", item.Key.JsonSerialize());
            }

            return true;

        }

        public async Task<bool> DeleteMapObjectGenCode(int objectCustomGenCodeMappingId)
        {

            var info = await _masterDbContext.ObjectCustomGenCodeMapping.FirstOrDefaultAsync(m => m.ObjectCustomGenCodeMappingId == objectCustomGenCodeMappingId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            _masterDbContext.ObjectCustomGenCodeMapping.Remove(info);
            await _masterDbContext.SaveChangesAsync();

            var objectName = ((EnumObjectType)info.ObjectTypeId).GetEnumDescription();

            await _activityLogService.CreateLog(EnumObjectType.ObjectCustomGenCodeMapping, objectCustomGenCodeMappingId, $"Loại bỏ cấu hình sinh mã khỏi đối tượng {objectName} {(info.ConfigObjectId > 0 ? (long?)info.ConfigObjectId : null)}", info.JsonSerialize());
            return true;

        }

        private IList<ObjectCustomGenCodeMapping> _objectCustomGenCodeMappings;
        private IList<CustomGenCode> _customGenCodes;

        public async Task<PageData<ObjectGenCodeMappingTypeModel>> GetObjectGenCodeMappingTypes(string keyword, int page, int size)
        {
            keyword = keyword?.ToLower();
            _objectCustomGenCodeMappings = await _masterDbContext.ObjectCustomGenCodeMapping.ToListAsync();
            _customGenCodes = await _masterDbContext.CustomGenCode.ToListAsync();

            var result = new List<ObjectGenCodeMappingTypeModel>();

            var organizationTask = OrganizationMappingTypeModels();

            var stockTask = StockMappingTypeModels();

            var purchasingOrderTask = PurchasingOrderMappingTypeModels();

            var vourcherTask = VourcherMappingTypeModels();

            var inputTask = InputMappingTypeModels();

            var manufactureTask = ManufactureMappingTypeModels();

            result.AddRange(await organizationTask);
            result.AddRange(await stockTask);
            result.AddRange(await purchasingOrderTask);
            result.AddRange(await vourcherTask);
            result.AddRange(await inputTask);
            result.AddRange(await manufactureTask);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(c =>
                 c.ObjectTypeName?.Contains(keyword) == true
                 || c.TargetObjectName?.Contains(keyword) == true
                 || c.TargetObjectTypeName?.Contains(keyword) == true
                 || c.FieldName?.Contains(keyword) == true
                 || c.CustomGenCodeName?.Contains(keyword) == true
                ).ToList();
            }
            var total = result.Count;

            return (result.Skip((page - 1) * size).Take(size).ToList(), total);
        }

        private async Task<IList<ObjectGenCodeMappingTypeModel>> StockMappingTypeModels()
        {
            var productTypesTask = _productHelperService.GetAllProductType();
            var stocksTask = _stockHelperService.GetAllStock();

            var result = new List<ObjectGenCodeMappingTypeModel>();
            foreach (var stock in await stocksTask)
            {
                result.Add(
                    GetObjectGenCodeMappingTypeModel(
                    moduleTypeId: EnumModuleType.Stock,
                    targeObjectTypeId: EnumObjectType.InventoryInput,
                    configObjectTypeId: EnumObjectType.Stock,
                    configObjectId: stock.StockId,
                    targetObjectName: stock.StockName,
                    fieldName: "Mã phiếu nhập")
                );

            }

            foreach (var stock in await stocksTask)
            {

                result.Add(
                    GetObjectGenCodeMappingTypeModel(
                    moduleTypeId: EnumModuleType.Stock,
                    targeObjectTypeId: EnumObjectType.InventoryOutput,
                    configObjectTypeId: EnumObjectType.Stock,
                    configObjectId: stock.StockId,
                    targetObjectName: stock.StockName,
                    fieldName: "Mã phiếu xuất")
                );

            }

            result.Add(
                   GetObjectGenCodeMappingTypeModel(
                   moduleTypeId: EnumModuleType.Stock,
                   targeObjectTypeId: EnumObjectType.Location,
                   fieldName: "Mã vị trí")
               );

            result.Add(
                   GetObjectGenCodeMappingTypeModel(
                   moduleTypeId: EnumModuleType.Stock,
                   targeObjectTypeId: EnumObjectType.Package,
                   fieldName: "Mã kiện")
               );


            foreach (var inputType in await productTypesTask)
            {
                result.Add(
                    GetObjectGenCodeMappingTypeModel(
                    moduleTypeId: EnumModuleType.Stock,
                    targeObjectTypeId: EnumObjectType.Product,
                    configObjectTypeId: EnumObjectType.ProductType,
                    configObjectId: inputType.ProductTypeId,
                    targetObjectName: inputType.ProductTypeName,
                    fieldName: "Mã sản phẩm")
                );
            }

            return result;
        }


        private Task<IList<ObjectGenCodeMappingTypeModel>> OrganizationMappingTypeModels()
        {

            IList<ObjectGenCodeMappingTypeModel> result = new List<ObjectGenCodeMappingTypeModel>();

            result.Add(
                 GetObjectGenCodeMappingTypeModel(
                 moduleTypeId: EnumModuleType.Organization,
                 targeObjectTypeId: EnumObjectType.Customer,
                 fieldName: "Mã đối tác")
             );


            result.Add(
                 GetObjectGenCodeMappingTypeModel(
                 moduleTypeId: EnumModuleType.Organization,
                 targeObjectTypeId: EnumObjectType.UserAndEmployee,
                 fieldName: "Mã nhân viên")
             );

            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.Organization,
                targeObjectTypeId: EnumObjectType.Department,
                fieldName: "Mã bộ phận")
            );

            return Task.FromResult(result);
        }

        private Task<IList<ObjectGenCodeMappingTypeModel>> PurchasingOrderMappingTypeModels()
        {

            IList<ObjectGenCodeMappingTypeModel> result = new List<ObjectGenCodeMappingTypeModel>();


            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.PurchaseOrder,
                targeObjectTypeId: EnumObjectType.PurchasingRequest,
                fieldName: "Mã YCVTHH")
            );

            result.Add(
              GetObjectGenCodeMappingTypeModel(
              moduleTypeId: EnumModuleType.PurchaseOrder,
              targeObjectTypeId: EnumObjectType.PurchasingSuggest,
              fieldName: "Mã đề nghị VTHH")
            );

            result.Add(
             GetObjectGenCodeMappingTypeModel(
             moduleTypeId: EnumModuleType.PurchaseOrder,
             targeObjectTypeId: EnumObjectType.PoAssignment,
             fieldName: "Mã phân công mua hàng")
           );

            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.PurchaseOrder,
                targeObjectTypeId: EnumObjectType.PurchaseOrder,
                fieldName: "Mã đơn đặt hàng")
            );

            return Task.FromResult(result);
        }


        private async Task<IList<ObjectGenCodeMappingTypeModel>> VourcherMappingTypeModels()
        {
            var voucherTypes = _voucherTypeHelperService.GetVoucherTypeSimpleList();

            var result = new List<ObjectGenCodeMappingTypeModel>();
            foreach (var voucherType in await voucherTypes)
            {
                foreach (var areaField in voucherType.AreaFields.Where(f => f.FormTypeId == EnumFormType.Generate))
                {
                    result.Add(
                        GetObjectGenCodeMappingTypeModel(
                        moduleTypeId: EnumModuleType.PurchaseOrder,
                        targeObjectTypeId: EnumObjectType.VoucherTypeRow,
                        configObjectTypeId: EnumObjectType.VoucherAreaField,
                        configObjectId: areaField.VoucherAreaFieldId,
                        targetObjectName: voucherType.Title,
                        fieldName: areaField.VoucherAreaFieldTitle)
                    );
                }
            }

            return result;
        }

        private async Task<IList<ObjectGenCodeMappingTypeModel>> InputMappingTypeModels()
        {
            var inputTypes = _inputTypeHelperService.GetInputTypeSimpleList();

            var result = new List<ObjectGenCodeMappingTypeModel>();
            foreach (var inputType in await inputTypes)
            {
                foreach (var areaField in inputType.AreaFields.Where(f => f.FormTypeId == EnumFormType.Generate))
                {
                    result.Add(
                        GetObjectGenCodeMappingTypeModel(
                        moduleTypeId: EnumModuleType.Accountant,
                        targeObjectTypeId: EnumObjectType.InputTypeRow,
                        configObjectTypeId: EnumObjectType.InputAreaField,
                        configObjectId: areaField.InputAreaFieldId,
                        targetObjectName: inputType.Title,
                        fieldName: areaField.InputAreaFieldTitle)
                    );
                }
            }

            return result;
        }

        private Task<IList<ObjectGenCodeMappingTypeModel>> ManufactureMappingTypeModels()
        {
            IList<ObjectGenCodeMappingTypeModel> result = new List<ObjectGenCodeMappingTypeModel>();

            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.Manufacturing,
                targeObjectTypeId: EnumObjectType.ProductionOrder,
                fieldName: "Mã LSX")
            );

            result.Add(
             GetObjectGenCodeMappingTypeModel(
             moduleTypeId: EnumModuleType.Manufacturing,
             targeObjectTypeId: EnumObjectType.OutsourceRequest,
             fieldName: "Mã yêu cầu gia công")
            );

            result.Add(
               GetObjectGenCodeMappingTypeModel(
               moduleTypeId: EnumModuleType.Manufacturing,
               targeObjectTypeId: EnumObjectType.OutsourceOrder,
               fieldName: "Mã đơn hàng gia công")
            );

            result.Add(
               GetObjectGenCodeMappingTypeModel(
               moduleTypeId: EnumModuleType.Manufacturing,
               targeObjectTypeId: EnumObjectType.ProductionSchedule,
               fieldName: "Mã kế hoạch sản xuất")
            );

            return Task.FromResult(result);
        }


        private ObjectGenCodeMappingTypeModel GetObjectGenCodeMappingTypeModel(
            EnumModuleType moduleTypeId,
            EnumObjectType targeObjectTypeId, EnumObjectType? configObjectTypeId = null, long configObjectId = 0,
            string targetObjectName = "", string fieldName = "")
        {
            configObjectTypeId = configObjectTypeId ?? targeObjectTypeId;

            var mapping = _objectCustomGenCodeMappings.FirstOrDefault(m => m.TargetObjectTypeId == (int)targeObjectTypeId && m.ConfigObjectTypeId == (int)configObjectTypeId && m.ConfigObjectId == configObjectId);
            var config = _customGenCodes.FirstOrDefault(c => c.CustomGenCodeId == mapping?.CustomGenCodeId);

            return new ObjectGenCodeMappingTypeModel()
            {
                ObjectCustomGenCodeMappingId = mapping?.ObjectCustomGenCodeMappingId,
                ModuleTypeId = moduleTypeId,
                ModuleTypeName = moduleTypeId.GetEnumDescription(),
                TargetObjectTypeId = targeObjectTypeId,
                TargetObjectTypeName = targeObjectTypeId.GetEnumDescription(),

                ConfigObjectTypeId = configObjectTypeId.Value,
                ObjectTypeId = configObjectTypeId.Value,
                ObjectTypeName = configObjectTypeId.Value.GetEnumDescription(),

                ConfigObjectId = configObjectId,
                TargetObjectName = targetObjectName,

                FieldName = fieldName,
                CustomGenCodeId = mapping?.CustomGenCodeId,
                CustomGenCodeName = config?.CustomGenCodeName
            };
        }

    }
}
