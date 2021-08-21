using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Model.FileResources;
using VErp.Commons.Enums.Stock;
using System.Data;
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

        public async Task<PageData<InventoryRequirementListModel>> GetListInventoryRequirements(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, bool? hasInventory, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var inventoryRequirementAsQuery = from ird in _stockDBContext.InventoryRequirementDetail
                                              join ir in _stockDBContext.InventoryRequirement on ird.InventoryRequirementId equals ir.InventoryRequirementId
                                              join @as in _stockDBContext.Stock on ird.AssignStockId equals @as.StockId into @asAlias
                                              from @as in @asAlias.DefaultIfEmpty()
                                              where ir.InventoryTypeId == (int)inventoryType
                                              select new
                                              {
                                                  InventoryRequirementCode = ir.InventoryRequirementCode,
                                                  Content = ir.Content,
                                                  Date = ir.Date,
                                                  DepartmentId = ird.DepartmentId,
                                                  ProductionStepId = ird.ProductionStepId,
                                                  CreatedByUserId = ir.CreatedByUserId,
                                                  ProductionOrderCode = ird.ProductionOrderCode,
                                                  Shipper = ir.Shipper,
                                                  CustomerId = ir.CustomerId,
                                                  BillForm = ir.BillForm,
                                                  BillCode = ir.BillCode,
                                                  BillSerial = ir.BillSerial,
                                                  BillDate = ir.BillDate,
                                                  ModuleTypeId = ir.ModuleTypeId,
                                                  InventoryRequirementId = ir.InventoryRequirementId,
                                                  CensorByUserId = ir.CensorByUserId,
                                                  CensorDatetimeUtc = ir.CensorDatetimeUtc,
                                                  CensorStatus = ir.CensorStatus,
                                                  StockName = @as != null ? @as.StockName : "",
                                                  ProductId = ird.ProductId
                                              };

            var inventoryAsQuery = from id in _stockDBContext.InventoryDetail
                                   join i in _stockDBContext.Inventory on id.InventoryId equals i.InventoryId
                                   select new
                                   {
                                       i.InventoryId,
                                       i.InventoryCode,
                                       id.InventoryDetailId,
                                       id.InventoryRequirementCode,
                                       id.ProductionOrderCode,
                                       i.DepartmentId,
                                       id.ProductId
                                   };

            var query = from ir in inventoryRequirementAsQuery
                        join p in _stockDBContext.Product on ir.ProductId equals p.ProductId into @pAlias
                        from p in @pAlias.DefaultIfEmpty()
                        where hasInventory.HasValue == false ? true : hasInventory.Value == false ? !inventoryAsQuery.Any(x => x.InventoryRequirementCode == ir.InventoryRequirementCode && x.ProductId == ir.ProductId) : inventoryAsQuery.Any(x => x.InventoryRequirementCode == ir.InventoryRequirementCode && x.ProductId == ir.ProductId)
                        select new
                        {
                            InventoryRequirementCode = ir.InventoryRequirementCode,
                            Content = ir.Content,
                            Date = ir.Date,
                            DepartmentId = ir.DepartmentId,
                            ProductionStepId = ir.ProductionStepId,
                            CreatedByUserId = ir.CreatedByUserId,
                            ProductionOrderCode = ir.ProductionOrderCode,
                            Shipper = ir.Shipper,
                            CustomerId = ir.CustomerId,
                            BillForm = ir.BillForm,
                            BillCode = ir.BillCode,
                            BillSerial = ir.BillSerial,
                            BillDate = ir.BillDate,
                            ModuleTypeId = ir.ModuleTypeId,
                            InventoryRequirementId = ir.InventoryRequirementId,
                            CensorByUserId = ir.CensorByUserId,
                            CensorDatetimeUtc = ir.CensorDatetimeUtc,
                            CensorStatus = ir.CensorStatus,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            StockName = ir.StockName,
                            ProductTitle = $"{p.ProductCode} / {p.ProductName}",
                            ProductId = ir.ProductId
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s => s.InventoryRequirementCode.Contains(keyword)
                                         || s.Content.Contains(keyword)
                                         || s.ProductionOrderCode.Contains(keyword)
                                         || s.ProductCode.Contains(keyword)
                                         || s.ProductName.Contains(keyword)
                );

            }
            query = query.InternalFilter(filters).InternalOrderBy(orderByFieldName, asc);

            var total = query.Count();
            var lst = (await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync())
                .Select(x => new InventoryRequirementListModel
                {
                    InventoryRequirementCode = x.InventoryRequirementCode,
                    Content = x.Content,
                    Date = x.Date.GetUnix(),
                    DepartmentId = x.DepartmentId,
                    ProductionStepId = x.ProductionStepId,
                    CreatedByUserId = x.CreatedByUserId,
                    ProductionOrderCode = x.ProductionOrderCode,
                    Shipper = x.Shipper,
                    CustomerId = x.CustomerId,
                    BillForm = x.BillForm,
                    BillCode = x.BillCode,
                    BillSerial = x.BillSerial,
                    BillDate = x.BillDate.GetUnix(),
                    ModuleTypeId = x.ModuleTypeId,
                    InventoryRequirementId = x.InventoryRequirementId,
                    CensorByUserId = x.CensorByUserId,
                    CensorDatetimeUtc = x.CensorDatetimeUtc.GetUnix(),
                    CensorStatus = (EnumInventoryRequirementStatus)x.CensorStatus,
                    ProductCode = x.ProductCode,
                    ProductName = x.ProductName,
                    StockName = x.StockName,
                    ProductTitle = x.ProductTitle,
                    ProductId = x.ProductId,
                    InventoryInfo = new List<InventorySimpleInfo>(),
                }).ToList();

            var lsInventoryRequirementCode = lst.Select(x => x.InventoryRequirementCode).ToArray();
            var mapInventorySimpleInfo = (await inventoryAsQuery.Where(x => lsInventoryRequirementCode.Contains(x.InventoryRequirementCode)).ToListAsync())
            .GroupBy(x => (new { x.InventoryRequirementCode, x.ProductId }).GetHashCode())
            .ToDictionary(k => k.Key, v => v.Select(x => new InventorySimpleInfo
            {
                InventoryCode = x.InventoryCode,
                InventoryId = x.InventoryId
            }).Distinct());

            foreach (var item in lst)
            {
                var hashCode = (new { item.InventoryRequirementCode, item.ProductId }).GetHashCode();
                if (mapInventorySimpleInfo.ContainsKey(hashCode))
                    ((List<InventorySimpleInfo>)item.InventoryInfo).AddRange(mapInventorySimpleInfo[hashCode]);
            }

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


            // Lấy thông tin xuất/nhập kho theo yêu cầu, mã lệnh SX, tổ nhận, sản phẩm
            // Do 1 phiếu yêu cầu tạo cho nhiều lệnh SX nên cần thêm thông tin mã lệnh SX, tổ nhận, sản phẩm để map chi tiết xuất/nhập kho nào với chi tiết yêu cầu nào
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
            // Map chi tiết xuất/nhập kho với chi tiết yêu cầu
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
