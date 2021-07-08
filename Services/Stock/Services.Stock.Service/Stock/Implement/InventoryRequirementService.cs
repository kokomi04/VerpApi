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

        public async Task<PageData<InventoryRequirementListModel>> GetListInventoryRequirements(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, bool hasInventory, Clause filters = null)
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
                || s.InventoryRequirement.Content.Contains(keyword)
                || s.OrderCode.Contains(keyword)
                || s.ProductionOrderCode.Contains(keyword)
                || s.OutsourceStepRequestCode.Contains(keyword)
                || s.Pocode.Contains(keyword)
                || s.Product.ProductCode.Contains(keyword)
                || s.Product.ProductName.Contains(keyword)
                || s.Product.ProductNameEng.Contains(keyword)

                );

            }
            query = query.InternalFilter(filters).InternalOrderBy(orderByFieldName, asc);

            var data = from q in query.AsEnumerable()
                       join id in _stockDBContext.InventoryDetail.Include(x => x.Inventory).AsEnumerable() on new { q.InventoryRequirement.InventoryRequirementCode, q.ProductionOrderCode, q.ProductId, q.DepartmentId } equals new { id.InventoryRequirementCode, id.ProductionOrderCode, id.ProductId, id.DepartmentId } into ids
                       from id in ids.DefaultIfEmpty()
                       select new { q, id } into t1
                       group t1 by new { t1.q.InventoryRequirement.InventoryRequirementCode, t1.q.ProductionOrderCode, t1.q.ProductId, t1.q.DepartmentId } into g
                       select new InventoryRequirementListModel
                       {
                           InventoryRequirementCode = g.FirstOrDefault().q.InventoryRequirement.InventoryRequirementCode,
                           Content = g.FirstOrDefault().q.InventoryRequirement.Content,
                           Date = g.FirstOrDefault().q.InventoryRequirement.Date.GetUnix(),
                           DepartmentId = g.FirstOrDefault().q.DepartmentId,
                           ProductionStepId = g.FirstOrDefault().q.ProductionStepId,
                           CreatedByUserId = g.FirstOrDefault().q.InventoryRequirement.CreatedByUserId,
                           ProductionOrderCode = g.FirstOrDefault().q.ProductionOrderCode,
                           Shipper = g.FirstOrDefault().q.InventoryRequirement.Shipper,
                           CustomerId = g.FirstOrDefault().q.InventoryRequirement.CustomerId,
                           BillForm = g.FirstOrDefault().q.InventoryRequirement.BillForm,
                           BillCode = g.FirstOrDefault().q.InventoryRequirement.BillCode,
                           BillSerial = g.FirstOrDefault().q.InventoryRequirement.BillSerial,
                           BillDate = g.FirstOrDefault().q.InventoryRequirement.BillDate.GetUnix(),
                           ModuleTypeId = g.FirstOrDefault().q.InventoryRequirement.ModuleTypeId,
                           InventoryRequirementId = g.FirstOrDefault().q.InventoryRequirement.InventoryRequirementId,
                           CensorByUserId = g.FirstOrDefault().q.InventoryRequirement.CensorByUserId,
                           CensorDatetimeUtc = g.FirstOrDefault().q.InventoryRequirement.CensorDatetimeUtc.GetUnix(),
                           CensorStatus = (EnumInventoryRequirementStatus)g.FirstOrDefault().q.InventoryRequirement.CensorStatus,
                           ProductCode = g.FirstOrDefault().q.Product?.ProductCode,
                           ProductName = g.FirstOrDefault().q.Product?.ProductName,
                           StockName = g.FirstOrDefault().q.AssignStock?.StockName,
                           ProductTitle = $"{g.FirstOrDefault().q.Product?.ProductCode} / {g.FirstOrDefault().q.Product?.ProductName}",
                           InventoryInfo = g.Where(x => x.id != null).Select(x => new InventorySimpleInfo
                           {
                               InventoryId = x.id.InventoryId,
                               InventoryCode = x.id.Inventory.InventoryCode
                           }).ToList()
                       };

            if (hasInventory)
            {
                data = data.Where(x => x.InventoryInfo != null && x.InventoryInfo.Count > 0);
            }

            var total = data.Count();
            var lst = (size > 0 ? data.Skip((page - 1) * size).Take(size) : data)
               .ToList();
            return (lst, total);
        }

        public async Task<long> GetInventoryRequirementId(EnumInventoryType inventoryType, string inventoryRequirementCode)
        {
            var entity = await _stockDBContext.InventoryRequirement
                .FirstOrDefaultAsync(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementCode == inventoryRequirementCode);
            return entity?.InventoryRequirementId ?? 0;
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

            var productionOrderCodes = entity.InventoryRequirementDetail.Select(ird => ird.ProductionOrderCode).Distinct().ToList();
            var departmentIds = entity.InventoryRequirementDetail.Select(ird => ird.DepartmentId).Distinct().ToList();
            var productIds = entity.InventoryRequirementDetail.Select(ird => ird.ProductId).Distinct().ToList();
            var inventoryMaps = _stockDBContext.InventoryDetail
                .Include(id => id.Inventory)
                .Where(id => model.InventoryRequirementCode == id.InventoryRequirementCode
                && productionOrderCodes.Contains(id.ProductionOrderCode)
                && departmentIds.Contains(id.Inventory.DepartmentId)
                && productIds.Contains(id.ProductId))
                .Select(id => new
                {
                    id.ProductId,
                    id.ProductionOrderCode,
                    id.Inventory.DepartmentId,
                    id.PrimaryQuantity,
                    id.InventoryId,
                    id.Inventory.InventoryCode
                })
                .ToList()
                .GroupBy(id => new
                {
                    id.ProductId,
                    id.ProductionOrderCode,
                    id.DepartmentId,
                })
                .ToDictionary(g => g.Key, g => new
                {
                    PrimaryQuantity = g.Sum(id => id.PrimaryQuantity),
                    InventorySimpleInfos = g.Select(id => new InventorySimpleInfo
                    {
                        InventoryId = id.InventoryId,
                        InventoryCode = id.InventoryCode
                    }).Distinct().ToList()
                });

            foreach (var data in inventoryMaps)
            {
                var quantity = data.Value.PrimaryQuantity;
                InventoryRequirementDetailOutputModel lastestDetail = null;
                foreach (var detail in model.InventoryRequirementDetail)
                {
                    if (detail.ProductId == data.Key.ProductId && detail.ProductionOrderCode == data.Key.ProductionOrderCode && detail.DepartmentId == data.Key.DepartmentId)
                    {
                        detail.InventoryInfo = data.Value.InventorySimpleInfos;
                        if (quantity <= 0) break;
                        detail.InventoryQuantity = quantity <= detail.PrimaryQuantity ? quantity : detail.PrimaryQuantity;
                        quantity = quantity - detail.InventoryQuantity;
                        lastestDetail = detail;
                    }
                }
                if (quantity > 0 && lastestDetail != null) lastestDetail.InventoryQuantity += quantity;
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
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

                var ctx = await GenerateInventoryRequirementCode(inventoryType, objectType, req);

                //CustomGenCodeOutputModel currentConfig = null;
                //if (string.IsNullOrEmpty(req.InventoryRequirementCode))
                //{
                //    currentConfig = await _customGenCodeHelperService.CurrentConfig(objectType, objectType, 0, null, null, req.Date);
                //    if (currentConfig == null)
                //    {
                //        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                //    }
                //    var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, null, req.Date);
                //    if (generated == null)
                //    {
                //        throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                //    }


                //    req.InventoryRequirementCode = generated.CustomCode;
                //}
                //else
                //{
                //    // Validate unique
                //    if (_stockDBContext.InventoryRequirement.Any(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementCode == req.InventoryRequirementCode))
                //        throw new BadRequestException(GeneralCode.InternalError, "Mã yêu cầu đã tồn tại");
                //}

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

                // Validate product
                var productIds = req.InventoryRequirementDetail.Select(d => d.ProductId).Distinct().ToList();
                if (_stockDBContext.Product.Where(p => productIds.Contains(p.ProductId)).Count() != productIds.Count)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Sản phẩm yêu cầu xuất/nhập kho là bán thành phẩm");

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


                await ctx.ConfirmCode();

                //await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);
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


        private async Task<GenerateCodeContext> GenerateInventoryRequirementCode(EnumInventoryType inventoryType, EnumObjectType objectTypeId, InventoryRequirementInputModel req)
        {
            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(objectTypeId)
                .SetConfigData(0, req.Date)
                .TryValidateAndGenerateCode(_stockDBContext.InventoryRequirement, req.InventoryRequirementCode, (s, code) => s.InventoryTypeId == (int)inventoryType && s.InventoryRequirementCode == code);

            req.InventoryRequirementCode = code;
            return ctx;
        }

        public async Task<long> UpdateInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, InventoryRequirementInputModel req)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

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
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

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
            var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

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

        public async Task<InventoryRequirementOutputModel> GetInventoryRequirementByProductionOrderId(EnumInventoryType inventoryType, string productionOrderCode, EnumInventoryRequirementType requirementType, int productMaterialsConsumptionGroupId)
        {
            var inventoryRequirements = await _stockDBContext.InventoryRequirement
                .Include(r => r.InventoryRequirementFile)
                .Include(r => r.InventoryRequirementDetail)
                .ThenInclude(d => d.ProductUnitConversion)
                .Where(r => r.InventoryTypeId == (int)inventoryType
                    && r.InventoryRequirementDetail.Any(rd => rd.ProductionOrderCode == productionOrderCode)
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
