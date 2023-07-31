using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.ObjectGenCode;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Config;
using static Verp.Resources.Master.Config.ObjectGenCode.ObjectGenCodeValidationMessage;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ObjectGenCodeService : IObjectGenCodeService
    {
        private readonly MasterDBContext _masterDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IGenCodeConfigService _genCodeConfigService;
        private readonly ICurrentContextService _currentContextService;

        private readonly IProductHelperService _productHelperService;
        private readonly IStockHelperService _stockHelperService;
        private readonly IInputPrivateTypeHelperService _inputPrivateTypeHelperService;
        private readonly IInputPublicTypeHelperService _inputPublicTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;
        private readonly ObjectActivityLogFacade _objectGenCodeActivityLog;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly ISalaryPeriodAdditionBillTypeHelperService _salaryPeriodAdditionBillTypeHelperService;

        public ObjectGenCodeService(MasterDBContext masterDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<ObjectGenCodeService> logger
            , IActivityLogService activityLogService
            , IGenCodeConfigService genCodeConfigService
            , ICurrentContextService currentContextService
            , IProductHelperService productHelperService
            , IStockHelperService stockHelperService
            , IInputPrivateTypeHelperService inputPrivateTypeHelperService
            , IInputPublicTypeHelperService inputPublicTypeHelperService
            , IVoucherTypeHelperService voucherTypeHelperService
            , IOrganizationHelperService organizationHelperService
            , ICategoryHelperService categoryHelperService
            , ISalaryPeriodAdditionBillTypeHelperService salaryPeriodAdditionBillTypeHelperService)
        {
            _masterDbContext = masterDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _genCodeConfigService = genCodeConfigService;
            _currentContextService = currentContextService;
            _productHelperService = productHelperService;
            _stockHelperService = stockHelperService;
            _inputPrivateTypeHelperService = inputPrivateTypeHelperService;
            _inputPublicTypeHelperService = inputPublicTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
            _objectGenCodeActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ObjectCustomGenCodeMapping);
            _organizationHelperService = organizationHelperService;
            _categoryHelperService = categoryHelperService;
            _salaryPeriodAdditionBillTypeHelperService = salaryPeriodAdditionBillTypeHelperService;
        }


        public async Task<CustomGenCodeOutputModel> GetCurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId, string configObjectTitle, long? fId, string code, long? date)
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
                throw CustomConfigNotExisted.BadRequestFormat(targetObjectTypeId.GetEnumDescription(), (configObjectId > 0 ? $"({configObjectId}) " : null) + configObjectTitle);

            }

            return await _genCodeConfigService.GetInfo(obj.CustomGenCodeId, fId, code, date);
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


            await _objectGenCodeActivityLog.LogBuilder(() => ObjectGenCodeActivityLogMessage.MapObjectGenCode)
             .MessageResourceFormatDatas(config.CustomGenCodeName)
             .ObjectId(obj.ObjectCustomGenCodeMappingId)
             .JsonData(model)
             .CreateLog();


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

                await _objectGenCodeActivityLog.LogBuilder(() => ObjectGenCodeActivityLogMessage.MapObjectGenCodeMulti)
                 .MessageResourceFormatDatas(item.Value.CustomGenCodeName)
                 .ObjectId(item.Key.ObjectCustomGenCodeMappingId)
                 .JsonData(item.Key)
                 .CreateLog();

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

            await _objectGenCodeActivityLog.LogBuilder(() => ObjectGenCodeActivityLogMessage.DeleteMapObjectGenCode)
              .MessageResourceFormatDatas(objectName, info.ConfigObjectId > 0 ? (long?)info.ConfigObjectId : null)
              .ObjectId(objectCustomGenCodeMappingId)
              .JsonData(info)
              .CreateLog();

            return true;

        }

        private IList<ObjectCustomGenCodeMapping> _objectCustomGenCodeMappings;
        private IList<CustomGenCode> _customGenCodes;

        public async Task<PageData<ObjectGenCodeMappingTypeModel>> GetObjectGenCodeMappingTypes(EnumModuleType? moduleTypeId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim().ToLower();

            _objectCustomGenCodeMappings = await _masterDbContext.ObjectCustomGenCodeMapping.ToListAsync();
            _customGenCodes = await _masterDbContext.CustomGenCode.ToListAsync();

            var result = new List<ObjectGenCodeMappingTypeModel>();

            var organizationTask = OrganizationMappingTypeModels();

            var stockTask = StockMappingTypeModels();

            var purchasingOrderTask = PurchasingOrderMappingTypeModels();

            var vourcherTask = VourcherMappingTypeModels();

            var inputPrivateTask = InputPrivateMappingTypeModels();

            var inputPublicTask = InputPublicMappingTypeModels();

            var manufactureTask = ManufactureMappingTypeModels();

            var masterTask = MasterMappingTypeModels();

            result.AddRange(await organizationTask);
            result.AddRange(await stockTask);
            result.AddRange(await purchasingOrderTask);
            result.AddRange(await vourcherTask);
            result.AddRange(await inputPrivateTask);
            result.AddRange(await inputPublicTask);
            result.AddRange(await manufactureTask);
            result.AddRange(await masterTask);

            if (moduleTypeId.HasValue)
            {
                result = result.Where(c => c.ModuleTypeId == moduleTypeId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(c =>
                 c.ObjectTypeName?.ToLower().Contains(keyword) == true
                 || c.TargetObjectName?.ToLower()?.Contains(keyword) == true
                 || c.TargetObjectTypeName?.ToLower()?.Contains(keyword) == true
                 || c.FieldName?.ToLower()?.Contains(keyword) == true
                 || c.CustomGenCodeName?.ToLower()?.Contains(keyword) == true
                ).ToList();
            }
            var total = result.Count;

            return (result.Skip((page - 1) * size).Take(size).ToList(), total);
        }

        private async Task<IList<ObjectGenCodeMappingTypeModel>> StockMappingTypeModels()
        {
            var result = new List<ObjectGenCodeMappingTypeModel>();

            result.Add(
                     GetObjectGenCodeMappingTypeModel(
                     moduleTypeId: EnumModuleType.Stock,
                     targeObjectTypeId: EnumObjectType.RequestInventoryInput,
                     fieldName: "Mã yêu cầu nhập kho")
                 );
            result.Add(
                   GetObjectGenCodeMappingTypeModel(
                   moduleTypeId: EnumModuleType.Stock,
                   targeObjectTypeId: EnumObjectType.RequestInventoryOutput,
                   fieldName: "Mã yêu cầu xuất kho")
               );



            var productTypesTask = _productHelperService.GetAllProductType();
            var stocksTask = _stockHelperService.GetAllStock();


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
                    fieldName: "Mã mặt hàng")
                );
            }

            result.Add(
                    GetObjectGenCodeMappingTypeModel(
                    moduleTypeId: EnumModuleType.Stock,
                    targeObjectTypeId: EnumObjectType.Product,
                    configObjectTypeId: EnumObjectType.Product,
                    fieldName: "Mã chi tiết mặt hàng")
                );

            result.Add(
                  GetObjectGenCodeMappingTypeModel(
                  moduleTypeId: EnumModuleType.Stock,
                  targeObjectTypeId: EnumObjectType.StockTakePeriod,
                  fieldName: "Mã kỳ kiểm kê")
              );
            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.Stock,
                targeObjectTypeId: EnumObjectType.StockTake,
                fieldName: "Mã phiếu kiểm kê")
            );
            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.Stock,
                targeObjectTypeId: EnumObjectType.StockTakeAcceptanceCertificate,
                fieldName: "Mã phiếu xử lý")
            );
            return result;
        }


        private async Task<IList<ObjectGenCodeMappingTypeModel>> OrganizationMappingTypeModels()
        {

            var hrTypes = await _organizationHelperService.GetHrTypeSimpleList();

            IList<ObjectGenCodeMappingTypeModel> result = new List<ObjectGenCodeMappingTypeModel>();


            foreach (var hrType in hrTypes)
            {
                foreach (var areaField in hrType.AreaFields.Where(f => f.FormTypeId == EnumFormType.Generate))
                {
                    result.Add(
                        GetObjectGenCodeMappingTypeModel(
                        moduleTypeId: EnumModuleType.Organization,
                        targeObjectTypeId: EnumObjectType.HrTypeRow,
                        configObjectTypeId: EnumObjectType.HrAreaField,
                        configObjectId: areaField.HrAreaFieldId,
                        targetObjectName: hrType.Title,
                        fieldName: areaField.HrAreaFieldTitle)
                    );
                    result.Add(
                        GetObjectGenCodeMappingTypeModel(
                        moduleTypeId: EnumModuleType.Organization,
                        targeObjectTypeId: EnumObjectType.HrTypeRow,
                        configObjectTypeId: EnumObjectType.HrArea,
                        configObjectId: areaField.HrAreaId,
                        targetObjectName: hrType.Title,
                        fieldName: areaField.HrAreaTitle)
                    );
                }
            }

            var salaryAdditionBillTypes = await _salaryPeriodAdditionBillTypeHelperService.ListTypes();
            foreach (var type in salaryAdditionBillTypes)
            {

                result.Add(
                       GetObjectGenCodeMappingTypeModel(
                       moduleTypeId: EnumModuleType.Organization,
                       targeObjectTypeId: EnumObjectType.SalaryPeriodAdditionBill,
                       configObjectTypeId: EnumObjectType.SalaryPeriodAdditionType,
                       configObjectId: type.SalaryPeriodAdditionTypeId,
                       targetObjectName: type.Title,
                       fieldName: "Số chứng từ")
                   );
            }



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

            return await Task.FromResult(result);
        }

        private Task<IList<ObjectGenCodeMappingTypeModel>> PurchasingOrderMappingTypeModels()
        {

            IList<ObjectGenCodeMappingTypeModel> result = new List<ObjectGenCodeMappingTypeModel>();


            result.Add(
                 GetObjectGenCodeMappingTypeModel(
                 moduleTypeId: EnumModuleType.PurchaseOrder,
                 targeObjectTypeId: EnumObjectType.MaterialCalc,
                 fieldName: "Mã số")
             );

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
                  targeObjectTypeId: EnumObjectType.PoProviderPricing,
                  fieldName: "Mã báo giá nhà cung cấp")
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

        private async Task<IList<ObjectGenCodeMappingTypeModel>> MasterMappingTypeModels()
        {
            var categories = _categoryHelperService.GetAllCategoryConfig();

            var result = new List<ObjectGenCodeMappingTypeModel>();
            foreach (var category in await categories)
            {
                foreach (var field in category.CategoryField.Where(f => f.FormTypeId == (int)EnumFormType.Generate))
                {
                    result.Add(
                        GetObjectGenCodeMappingTypeModel(
                        moduleTypeId: EnumModuleType.Master,
                        targeObjectTypeId: EnumObjectType.Category,
                        configObjectTypeId: EnumObjectType.CategoryField,
                        configObjectId: field.CategoryFieldId,
                        targetObjectName: category.Title,
                        fieldName: field.CategoryFieldName)
                    );
                }
            }

            return result;
        }

        private async Task<IList<ObjectGenCodeMappingTypeModel>> InputPrivateMappingTypeModels()
        {
            var inputTypes = _inputPrivateTypeHelperService.GetInputTypeSimpleList();

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

        private async Task<IList<ObjectGenCodeMappingTypeModel>> InputPublicMappingTypeModels()
        {
            var inputTypes = _inputPublicTypeHelperService.GetInputTypeSimpleList();

            var result = new List<ObjectGenCodeMappingTypeModel>();
            foreach (var inputType in await inputTypes)
            {
                foreach (var areaField in inputType.AreaFields.Where(f => f.FormTypeId == EnumFormType.Generate))
                {
                    result.Add(
                        GetObjectGenCodeMappingTypeModel(
                        moduleTypeId: EnumModuleType.AccountantPublic,
                        targeObjectTypeId: EnumObjectType.InputTypeRowPublic,
                        configObjectTypeId: EnumObjectType.InputAreaFieldPublic,
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
             targeObjectTypeId: EnumObjectType.OutsourceRequestPart,
             fieldName: "Mã yêu cầu gia công chi tiết")
            );

            result.Add(
               GetObjectGenCodeMappingTypeModel(
               moduleTypeId: EnumModuleType.Manufacturing,
               targeObjectTypeId: EnumObjectType.OutsourceRequestStep,
               fieldName: "Mã yêu cầu gia công công đoạn")
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

            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.Manufacturing,
                targeObjectTypeId: EnumObjectType.PropertyCalc,
                fieldName: "Mã số")
            );

            result.Add(
                GetObjectGenCodeMappingTypeModel(
                moduleTypeId: EnumModuleType.Manufacturing,
                targeObjectTypeId: EnumObjectType.ProductionMaterialsRequirement,
                fieldName: "Mã yêu cầu vật tư thêm")
            );

            result.Add(
               GetObjectGenCodeMappingTypeModel(
               moduleTypeId: EnumModuleType.Manufacturing,
               targeObjectTypeId: EnumObjectType.ProductionHandoverReceipt,
               fieldName: "Mã phiếu thống kê sản xuất")
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
