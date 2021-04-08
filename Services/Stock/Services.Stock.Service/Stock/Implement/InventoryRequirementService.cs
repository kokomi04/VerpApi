using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using Microsoft.Data.SqlClient;
using VErp.Services.Stock.Service.Stock;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using System.Linq.Expressions;
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Model.FileResources;
using VErp.Commons.Enums.Stock;
using VErp.Commons.GlobalObject.InternalDataInterface;
using System.Data;
using System.Reflection;
using VErp.Commons.Enums.StockEnum;

namespace VErp.Services.Manafacturing.Service.Stock.Implement
{
    public class InventoryRequirementService : IInventoryRequirementService
    {
        private readonly StockDBContext _stockDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly IFileService _fileService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IOutsideMappingHelperService _outsideMappingHelperService;
        private readonly IProductionOrderHelperService _productionOrderHelperService;

        public InventoryRequirementService(StockDBContext stockDBContext
            , IActivityLogService activityLogService
            , ILogger<InventoryRequirementService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , IFileService fileService
            , ICurrentContextService currentContextService
            , IOutsideMappingHelperService outsideMappingHelperService
            , IProductionOrderHelperService productionOrderHelperService
            )
        {
            _stockDBContext = stockDBContext;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _fileService = fileService;
            _currentContextService = currentContextService;
            _outsideMappingHelperService = outsideMappingHelperService;
            _productionOrderHelperService = productionOrderHelperService;
        }

        public async Task<PageData<InventoryRequirementListModel>> GetListInventoryRequirements(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _stockDBContext.InventoryRequirementDetail
                .Include(s => s.InventoryRequirement)
                .Include(s => s.AssignStock)
                .Include(s => s.Product)
                .Where(s => s.InventoryRequirement.InventoryTypeId == (int)inventoryType);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s => s.InventoryRequirement.InventoryRequirementCode.Contains(keyword)
                || s.InventoryRequirement.Content.Contains(keyword));
            }

            query = query.InternalFilter(filters, _currentContextService.TimeZoneOffset).InternalOrderBy(orderByFieldName, asc);

            var result = query.ProjectTo<InventoryRequirementListModel>(_mapper.ConfigurationProvider);

            var lst = await (size > 0 ? result.Skip((page - 1) * size).Take(size) : result)
                .ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<InventoryRequirementOutputModel> GetInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId)
        {
            var entity = _stockDBContext.InventoryRequirement
                .Include(r => r.InventoryRequirementFile)
                .Include(r => r.InventoryRequirementDetail)
                .ThenInclude(d => d.ProductUnitConversion)
                .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);
            var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
            if (entity == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");
            var model = _mapper.Map<InventoryRequirementOutputModel>(entity);

            var inventoryRequirementDetailIds = model.InventoryRequirementDetail.Select(d => d.InventoryRequirementDetailId).ToList();

            var inventoryDetailQuantitys = _stockDBContext.InventoryDetail
                .Include(id => id.Inventory)
                .Where(id => id.InventoryRequirementDetailId.HasValue && id.Inventory.IsApproved && inventoryRequirementDetailIds.Contains(id.InventoryRequirementDetailId.Value))
                .GroupBy(id => id.InventoryRequirementDetailId)
                .Select(g => new
                {
                    InventoryRequirementDetailId = g.Key,
                    PrimaryQuantity = g.Sum(id => id.PrimaryQuantity)
                })
                .ToDictionary(g => g.InventoryRequirementDetailId, g => g.PrimaryQuantity);

            foreach (var inventoryRequirementDetail in model.InventoryRequirementDetail)
            {
                if (inventoryDetailQuantitys.ContainsKey(inventoryRequirementDetail.InventoryRequirementDetailId))
                    inventoryRequirementDetail.InventoryQuantity = inventoryDetailQuantitys[inventoryRequirementDetail.InventoryRequirementDetailId];
            }

            var fileIds = model.InventoryRequirementFile.Select(q => q.FileId).ToList();

            var attachedFiles = await _fileService.GetListFileUrl(fileIds, EnumThumbnailSize.Large);
            if (attachedFiles == null)
            {
                attachedFiles = new List<FileToDownloadInfo>();
            }
            foreach (var item in model.InventoryRequirementFile)
            {
                item.FileToDownloadInfo = attachedFiles.FirstOrDefault(f => f.FileId == item.FileId);
            }
            return model;
        }

        public async Task<long> AddInventoryRequirement(EnumInventoryType inventoryType, InventoryRequirementInputModel req)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;
                CustomGenCodeOutputModel currentConfig = null;
                if (string.IsNullOrEmpty(req.InventoryRequirementCode))
                {
                    currentConfig = await _customGenCodeHelperService.CurrentConfig(objectType, objectType, 0, null, null, req.Date);
                    if (currentConfig == null)
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                    }
                    var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, null, req.Date);
                    if (generated == null)
                    {
                        throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                    }


                    req.InventoryRequirementCode = generated.CustomCode;
                }
                else
                {
                    // Validate unique
                    if (_stockDBContext.InventoryRequirement.Any(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementCode == req.InventoryRequirementCode))
                        throw new BadRequestException(GeneralCode.InternalError, "Mã yêu cầu đã tồn tại");
                }

                // validate product duplicate
                if (req.InventoryRequirementDetail.GroupBy(d => new { d.ProductId, d.DepartmentId, d.ProductionStepId }).Any(g => g.Count() > 1))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại sản phẩm trùng nhau trong phiếu yêu cầu");

                await ValidateInventoryRequirementConfig(req.Date.UnixToDateTime(), null);
                var inventoryRequirement = _mapper.Map<InventoryRequirementEntity>(req);
                inventoryRequirement.InventoryTypeId = (int)inventoryType;
                inventoryRequirement.CensorStatus = (int)EnumInventoryRequirementStatus.Waiting;

                _stockDBContext.InventoryRequirement.Add(inventoryRequirement);
                await _stockDBContext.SaveChangesAsync();

                // Tạo file
                foreach (var item in req.InventoryRequirementFile)
                {
                    // Tạo mới
                    var inventoryRequirementFile = _mapper.Map<InventoryRequirementFile>(item);
                    inventoryRequirementFile.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDBContext.InventoryRequirementFile.Add(inventoryRequirementFile);
                }

                // Tạo detail
                foreach (var item in req.InventoryRequirementDetail)
                {
                    // Tạo mới
                    var inventoryRequirementDetail = _mapper.Map<InventoryRequirementDetail>(item);
                    inventoryRequirementDetail.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDBContext.InventoryRequirementDetail.Add(inventoryRequirementDetail);
                }
                await _stockDBContext.SaveChangesAsync();
                trans.Commit();



                await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);
                await _activityLogService.CreateLog(objectType, inventoryRequirement.InventoryRequirementId, $"Thêm mới dữ liệu yêu cầu xuất/nhập kho {inventoryRequirement.InventoryRequirementCode}", req.JsonSerialize());

                if (!string.IsNullOrWhiteSpace(req?.OutsideImportMappingData?.MappingFunctionKey))
                {
                    await _outsideMappingHelperService.MappingObjectCreate(req.OutsideImportMappingData.MappingFunctionKey, req.OutsideImportMappingData.ObjectId, objectType, inventoryRequirement.InventoryRequirementId);
                }


                return inventoryRequirement.InventoryRequirementId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateInventoryRequirement");
                throw;
            }
        }

        public async Task<long> UpdateInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, InventoryRequirementInputModel req)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;

                var inventoryRequirement = _stockDBContext.InventoryRequirement
                    .Include(r => r.InventoryRequirementDetail)
                    .Include(r => r.InventoryRequirementFile)
                    .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

                var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
                if (inventoryRequirement == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");

                if (inventoryRequirement.InventoryRequirementCode != req.InventoryRequirementCode)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được thay đổi mã phiếu yêu cầu");

                //if (inventoryRequirement.ProductionOrderId.HasValue)
                //    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được thay đổi phiếu yêu cầu từ sản xuất");

                // validate product duplicate
                if (req.InventoryRequirementDetail.GroupBy(d => new { d.ProductId, d.DepartmentId, d.ProductionStepId }).Any(g => g.Count() > 1))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại sản phẩm trùng nhau trong phiếu yêu cầu");

                await ValidateInventoryRequirementConfig(req.Date.UnixToDateTime(), inventoryRequirement.Date);

                _mapper.Map(req, inventoryRequirement);
                inventoryRequirement.CensorStatus = (int)EnumInventoryRequirementStatus.Waiting;
                inventoryRequirement.CensorByUserId = null;
                inventoryRequirement.CensorDatetimeUtc = null;
                // Cập nhật file
                var createFiles = req.InventoryRequirementFile.Where(f => !inventoryRequirement.InventoryRequirementFile.Any(of => of.FileId == f.FileId)).ToList();
                var deleteFiles = inventoryRequirement.InventoryRequirementFile.Where(f => !req.InventoryRequirementFile.Any(of => of.FileId == f.FileId)).ToList();
                // Tạo mới
                foreach (var item in createFiles)
                {
                    var inventoryRequirementFile = _mapper.Map<InventoryRequirementFile>(item);
                    inventoryRequirementFile.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDBContext.InventoryRequirementFile.Add(inventoryRequirementFile);
                }
                // Xóa
                foreach (var item in deleteFiles)
                {
                    item.IsDeleted = true;
                }

                // Cập nhật detail
                var newDetails = new List<InventoryRequirementDetailInputModel>(req.InventoryRequirementDetail);
                var oldDetails = new List<InventoryRequirementDetail>(inventoryRequirement.InventoryRequirementDetail);

                foreach (var item in newDetails)
                {
                    if (item.InventoryRequirementDetailId == 0)
                    {
                        // Tạo mới
                        var inventoryRequirementDetail = _mapper.Map<InventoryRequirementDetail>(item);
                        inventoryRequirementDetail.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                        _stockDBContext.InventoryRequirementDetail.Add(inventoryRequirementDetail);
                    }
                    else // Cập nhật
                    {
                        var entity = oldDetails.First(d => d.InventoryRequirementDetailId == item.InventoryRequirementDetailId);
                        _mapper.Map(item, entity);
                        oldDetails.Remove(entity);
                    }
                }
                // Xóa
                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                }

                await _stockDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(objectType, inventoryRequirement.InventoryRequirementId, $"Cập nhật dữ liệu yêu cầu xuất/nhập kho {inventoryRequirement.InventoryRequirementCode}", req.JsonSerialize());
                return inventoryRequirementId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateInventoryRequirement");
                throw;
            }
        }

        public async Task<bool> DeleteInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;

                var inventoryRequirement = _stockDBContext.InventoryRequirement
                    .Include(r => r.InventoryRequirementDetail)
                    .Include(r => r.InventoryRequirementFile)
                    .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

                var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
                if (inventoryRequirement == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");

                //if (inventoryRequirement.ProductionOrderId.HasValue)
                //    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được xóa phiếu yêu cầu từ sản xuất");

                await ValidateInventoryRequirementConfig(inventoryRequirement.Date, inventoryRequirement.Date);

                inventoryRequirement.IsDeleted = true;

                // Xóa file
                foreach (var item in inventoryRequirement.InventoryRequirementFile)
                {
                    item.IsDeleted = true;
                }

                // Xóa detail
                foreach (var item in inventoryRequirement.InventoryRequirementDetail)
                {
                    item.IsDeleted = true;
                }

                await _stockDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(objectType, inventoryRequirement.InventoryRequirementId, $"Xóa dữ liệu yêu cầu xuất/nhập kho {inventoryRequirement.InventoryRequirementCode}", inventoryRequirement.JsonSerialize());

                await _outsideMappingHelperService.MappingObjectDelete(objectType, inventoryRequirementId);

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteInventoryRequirement");
                throw;
            }
        }

        public async Task<bool> ConfirmInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, EnumInventoryRequirementStatus status, Dictionary<long, int> assignStocks = null)
        {
            var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;

            var inventoryRequirement = _stockDBContext.InventoryRequirement
                .Include(r => r.InventoryRequirementDetail)
                .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

            var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
            if (inventoryRequirement == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");

            if (status == EnumInventoryRequirementStatus.Accepted && inventoryRequirement.CensorStatus == (int)EnumInventoryRequirementStatus.Accepted)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Phiếu yêu cầu {type} đã được duyệt");

            if (status == EnumInventoryRequirementStatus.Rejected && inventoryRequirement.CensorStatus != (int)EnumInventoryRequirementStatus.Waiting)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Phiếu yêu cầu {type} không phải đơn chờ duyệt");

            // Validate assign stock
            if (status == EnumInventoryRequirementStatus.Accepted && (assignStocks == null || inventoryRequirement.InventoryRequirementDetail.Any(d => !assignStocks.ContainsKey(d.InventoryRequirementDetailId))))
                throw new BadRequestException(GeneralCode.InvalidParams, $"Phiếu yêu cầu {type} chưa chỉ định đủ kho xuất cho mặt hàng");

            await ValidateInventoryRequirementConfig(inventoryRequirement.Date, inventoryRequirement.Date);

            inventoryRequirement.CensorStatus = (int)status;
            inventoryRequirement.CensorByUserId = _currentContextService.UserId;
            inventoryRequirement.CensorDatetimeUtc = DateTime.UtcNow;
            if (status == EnumInventoryRequirementStatus.Accepted)
            {
                foreach (var item in inventoryRequirement.InventoryRequirementDetail)
                {
                    item.AssignStockId = assignStocks[item.InventoryRequirementDetailId];
                }
            }
            await _stockDBContext.SaveChangesAsync();
            return true;
        }


        protected async Task ValidateInventoryRequirementConfig(DateTime? billDate, DateTime? oldDate)
        {
            if (billDate != null || oldDate != null)
            {

                var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
                var sqlParams = new List<SqlParameter>
                {
                    result
                };

                if (oldDate.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@OldDate", SqlDbType.DateTime2) { Value = oldDate });
                }

                if (billDate.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@BillDate", SqlDbType.DateTime2) { Value = billDate });
                }

                await _stockDBContext.ExecuteStoreProcedure("asp_ValidateBillDate", sqlParams, true);

                if (!(result.Value as bool?).GetValueOrDefault())
                    throw new BadRequestException(GeneralCode.InvalidParams, "Ngày chứng từ không được phép trước ngày chốt sổ");
            }
        }

        public async Task<InventoryRequirementOutputModel> GetInventoryRequirementByProductionOrderId(EnumInventoryType inventoryType, long productionOrderId, EnumInventoryRequirementType requirementType, int productMaterialsConsumptionGroupId)
        {
            var inventoryRequirements = await _stockDBContext.InventoryRequirement
                .Include(r => r.InventoryRequirementFile)
                .Include(r => r.InventoryRequirementDetail)
                .ThenInclude(d => d.ProductUnitConversion)
                .Where(r => r.InventoryTypeId == (int)inventoryType
                    && r.ProductionOrderId == productionOrderId
                    && r.InventoryRequirementTypeId == (int)EnumInventoryRequirementType.Complete)
                .ToListAsync();

            var entity = inventoryRequirements.FirstOrDefault(x => x.ProductMaterialsConsumptionGroupId.GetValueOrDefault() == productMaterialsConsumptionGroupId);

            var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";

            if (entity == null)
                return null;

            var model = _mapper.Map<InventoryRequirementOutputModel>(entity);

            var fileIds = model.InventoryRequirementFile.Select(q => q.FileId).ToList();

            var attachedFiles = await _fileService.GetListFileUrl(fileIds, EnumThumbnailSize.Large);

            if (attachedFiles == null)
                attachedFiles = new List<FileToDownloadInfo>();

            foreach (var item in model.InventoryRequirementFile)
            {
                item.FileToDownloadInfo = attachedFiles.FirstOrDefault(f => f.FileId == item.FileId);
            }
            return model;
        }
    }
}
