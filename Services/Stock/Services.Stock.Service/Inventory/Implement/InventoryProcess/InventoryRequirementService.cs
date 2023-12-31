﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Stock.InventoryProcess;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.Stock;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Abstract;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Inventory.Implement.Abstract;
using VErp.Services.Stock.Service.Stock;
using static Verp.Resources.Stock.InventoryProcess.InventoryRequirementMessage;
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;

namespace VErp.Services.Manafacturing.Service.Stock.Implement
{
    public class InventoryRequirementService : BillDateValidateionServiceAbstract, IInventoryRequirementService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IFileService _fileService;
        private readonly IOutsideMappingHelperService _outsideMappingHelperService;
        private readonly ObjectActivityLogFacade _invRequestActivityLog;
        private readonly StockDBContext _stockDbContext;
        private readonly ICurrentContextService _currentContextService;
        public InventoryRequirementService(StockDBContext stockDBContext
            , IActivityLogService activityLogService
            , ILogger<InventoryRequirementService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IFileService fileService
            , ICurrentContextService currentContextService
            , IOutsideMappingHelperService outsideMappingHelperService
            ) : base(stockDBContext)
        {
            _stockDbContext = stockDBContext;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _fileService = fileService;
            _outsideMappingHelperService = outsideMappingHelperService;
            _currentContextService = currentContextService;
            _invRequestActivityLog = activityLogService.CreateObjectTypeActivityLog(null);
        }

        public async Task<PageData<InventoryRequirementListModel>> GetList(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, bool? hasInventory, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var sumInvs = from ird in _stockDbContext.InventoryRequirementDetail
                          join invd in _stockDbContext.InventoryDetail on ird.InventoryRequirementDetailId equals invd.InventoryRequirementDetailId into invds
                          from invd in invds.DefaultIfEmpty()
                          group invd by ird.InventoryRequirementDetailId into d
                          select new
                          {
                              InventoryRequirementDetailId = d.Key,
                              InventoryQuantity = d.Sum(v => v.PrimaryQuantity)
                          };

            var remainings = from sp in _stockDbContext.StockProduct
                             group sp by sp.ProductId into d
                             select new
                             {
                                 ProductId = d.Key,
                                 PrimaryQuantityRemaining = d.Sum(v => v.PrimaryQuantityRemaining)
                             };

            var inventoryRequirementAsQuery = from ird in _stockDbContext.InventoryRequirementDetail
                                              join s in sumInvs on ird.InventoryRequirementDetailId equals s.InventoryRequirementDetailId
                                              join ir in _stockDbContext.InventoryRequirement on ird.InventoryRequirementId equals ir.InventoryRequirementId
                                              join @as in _stockDbContext.Stock on ird.AssignStockId equals @as.StockId into @asAlias
                                              from @as in @asAlias.DefaultIfEmpty()
                                              join r in remainings on ird.ProductId equals r.ProductId into rs
                                              from r in rs.DefaultIfEmpty()
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

                                                  ird.OutsourceStepRequestId,

                                                  ird.OrderCode,
                                                  ird.Pocode,

                                                  //Shipper = ir.Shipper,
                                                  CustomerId = ir.CustomerId,
                                                  // BillForm = ir.BillForm,
                                                  //BillCode = ir.BillCode,
                                                  //BillSerial = ir.BillSerial,
                                                  //BillDate = ir.BillDate,
                                                  ModuleTypeId = ir.ModuleTypeId,
                                                  InventoryRequirementId = ir.InventoryRequirementId,
                                                  CensorByUserId = ir.CensorByUserId,
                                                  CensorDatetimeUtc = ir.CensorDatetimeUtc,
                                                  CensorStatus = ir.CensorStatus,
                                                  StockName = @as != null ? @as.StockName : "",
                                                  ProductId = ird.ProductId,
                                                  InventoryRequirementDetailId = ird.InventoryRequirementDetailId,

                                                  ird.PrimaryQuantity,
                                                  ird.ProductUnitConversionQuantity,

                                                  ird.ProductUnitConversionId,



                                                  s.InventoryQuantity,
                                                  PrimaryQuantityRemaining = r == null ? (decimal?)null : r.PrimaryQuantityRemaining,

                                                  ir.UpdatedDatetimeUtc
                                              };




            if (hasInventory.HasValue)
            {
                var invRequirementDetailIds = _stockDbContext.InventoryDetail.Select(d => d.InventoryRequirementDetailId);
                if (hasInventory.Value)
                {
                    inventoryRequirementAsQuery = inventoryRequirementAsQuery.Where(r => invRequirementDetailIds.Contains(r.InventoryRequirementDetailId));
                }
                else
                {
                    inventoryRequirementAsQuery = inventoryRequirementAsQuery.Where(r => !invRequirementDetailIds.Contains(r.InventoryRequirementDetailId));
                }
            }
            var query = from ir in inventoryRequirementAsQuery
                        join p in _stockDbContext.Product on ir.ProductId equals p.ProductId into @pAlias
                        from p in @pAlias.DefaultIfEmpty()

                        select new
                        {
                            ir.InventoryRequirementCode,
                            ir.Content,
                            ir.Date,
                            DepartmentId = ir.DepartmentId,
                            ProductionStepId = ir.ProductionStepId,
                            CreatedByUserId = ir.CreatedByUserId,
                            ProductionOrderCode = ir.ProductionOrderCode,
                            ir.OutsourceStepRequestId,
                            ir.OrderCode,
                            ir.Pocode,
                            //ir.Shipper,
                            ir.CustomerId,
                            //ir.BillForm,
                            //ir.BillCode,
                            //ir.BillSerial,
                            //ir.BillDate,
                            ir.ModuleTypeId,
                            ir.InventoryRequirementId,
                            ir.CensorByUserId,
                            ir.CensorDatetimeUtc,
                            ir.CensorStatus,
                            p.ProductCode,
                            p.ProductName,
                            ir.StockName,
                            ProductTitle = $"{p.ProductCode} / {p.ProductName}",
                            ir.ProductId,
                            ir.InventoryRequirementDetailId,

                            ir.PrimaryQuantity,
                            ir.ProductUnitConversionQuantity,

                            ir.ProductUnitConversionId,

                            ir.InventoryQuantity,
                            ir.PrimaryQuantityRemaining,
                            ir.UpdatedDatetimeUtc
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
                    OutsourceStepRequestId = x.OutsourceStepRequestId,
                    OrderCode = x.OrderCode,
                    PoCode = x.Pocode,
                    //Shipper = x.Shipper,
                    CustomerId = x.CustomerId,
                    //BillForm = x.BillForm,
                    //BillCode = x.BillCode,
                    //BillSerial = x.BillSerial,
                    //BillDate = x.BillDate.GetUnix(),
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
                    InventoryRequirementDetailId = x.InventoryRequirementDetailId,
                    InventoryInfo = new List<InventorySimpleInfo>(),
                    PrimaryQuantityRemaining = x.PrimaryQuantityRemaining ?? 0,
                    InventoryQuantity = x.InventoryQuantity,
                    PrimaryQuantity = x.PrimaryQuantity,
                    ProductUnitConversionId = x.ProductUnitConversionId,
                    ProductUnitConversionQuantity = x.ProductUnitConversionQuantity,
                    UpdatedDatetimeUtc = x.UpdatedDatetimeUtc.GetUnix(),

                }).ToList();

            var inventoryRequirementDetailIds = lst.Select(d => (long?)d.InventoryRequirementDetailId).ToList();

            var inventoryInfos = (await (from id in _stockDbContext.InventoryDetail
                                         join i in _stockDbContext.Inventory on id.InventoryId equals i.InventoryId
                                         where inventoryRequirementDetailIds.Contains(id.InventoryRequirementDetailId)
                                         select new
                                         {
                                             i.InventoryId,
                                             i.InventoryCode,
                                             id.InventoryDetailId,
                                             id.InventoryRequirementDetailId,
                                             //id.InventoryRequirementCode,
                                             id.ProductionOrderCode,
                                             i.DepartmentId,
                                             id.ProductId
                                         }).ToListAsync()
                                   ).GroupBy(d => d.InventoryRequirementDetailId)
                                   .ToDictionary(
                                        d => d.Key,
                                        d => d.Select(iv => new InventorySimpleInfo { InventoryId = iv.InventoryId, InventoryCode = iv.InventoryCode }).ToList()
                                        );

            foreach (var item in lst)
            {

                if (inventoryInfos.ContainsKey(item.InventoryRequirementDetailId))
                    ((List<InventorySimpleInfo>)item.InventoryInfo).AddRange(inventoryInfos[item.InventoryRequirementDetailId]);
            }

            return (lst, total);
        }

        public async Task<long> GetIdByCode(EnumInventoryType inventoryType, string inventoryRequirementCode)
        {
            var entity = await _stockDbContext.InventoryRequirement
                .FirstOrDefaultAsync(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementCode == inventoryRequirementCode);
            return entity?.InventoryRequirementId ?? 0;
        }

        public async Task<InventoryRequirementOutputModel> Info(EnumInventoryType inventoryType, long inventoryRequirementId)
        {
            var entity = await GetByIds(inventoryType, new[] { inventoryRequirementId });
            if (entity.Count == 0) throw InvRequestNotFound.BadRequest();
            return entity.First();
        }

        public async Task<IList<InventoryRequirementOutputModel>> GetByIds(EnumInventoryType inventoryType, IList<long> inventoryRequirementIds)
        {
            var entity = await _stockDbContext.InventoryRequirement
                .Include(r => r.InventoryRequirementFile)
                .Include(r => r.InventoryRequirementDetail)
                .ThenInclude(d => d.ProductUnitConversion)
                .Where(r => r.InventoryTypeId == (int)inventoryType && inventoryRequirementIds.Contains(r.InventoryRequirementId))
                .ToListAsync();


            var lst = _mapper.Map<List<InventoryRequirementOutputModel>>(entity);


            // Lấy thông tin xuất/nhập kho theo yêu cầu, mã lệnh SX, tổ nhận, sản phẩm
            // Do 1 phiếu yêu cầu tạo cho nhiều lệnh SX nên cần thêm thông tin mã lệnh SX, tổ nhận, sản phẩm để map chi tiết xuất/nhập kho nào với chi tiết yêu cầu nào
            //var productionOrderCodes = entity.InventoryRequirementDetail.Select(ird => ird.ProductionOrderCode).Distinct().ToList();
            //var departmentIds = entity.InventoryRequirementDetail.Select(ird => ird.DepartmentId).Distinct().ToList();
            //var productIds = entity.InventoryRequirementDetail.Select(ird => ird.ProductId).Distinct().ToList();
            var inventoryRequirementDetailIds = lst.SelectMany(r => r.InventoryRequirementDetail.Select(ird => ird.InventoryRequirementDetailId)).ToList();

            // Lấy thông tin xuất/nhập kho theo ID chi tiết yêu cầu
            var inventoryMaps = _stockDbContext.InventoryDetail
                .Include(id => id.Inventory)
                .Where(id => id.InventoryRequirementDetailId.HasValue
                && inventoryRequirementDetailIds.Contains(id.InventoryRequirementDetailId.Value))
                .Select(id => new
                {
                    InventoryRequirementDetailId = id.InventoryRequirementDetailId.Value,
                    id.Inventory.InventoryCode,
                    id.Inventory.InventoryId,
                    id.PrimaryQuantity
                })
                .ToList()
                .GroupBy(id => id.InventoryRequirementDetailId)
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
                foreach (var model in lst)
                {
                    var quantity = data.Value.PrimaryQuantity;
                    InventoryRequirementDetailOutputModel lastestDetail = null;
                    foreach (var detail in model.InventoryRequirementDetail)
                    {
                        if (detail.InventoryRequirementDetailId == data.Key)
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
            }

            var fileIds = lst.SelectMany(r => r.InventoryRequirementFile.Select(q => q.FileId)).ToList();

            var attachedFiles = await _fileService.GetListFileUrl(fileIds, EnumThumbnailSize.Large);
            if (attachedFiles == null)
            {
                attachedFiles = new List<FileToDownloadInfo>();
            }

            foreach (var model in lst)
            {
                foreach (var item in model.InventoryRequirementFile)
                {
                    item.FileToDownloadInfo = attachedFiles.FirstOrDefault(f => f.FileId == item.FileId);
                }
            }
            return lst;
        }

        public async Task<long> Create(EnumInventoryType inventoryType, InventoryRequirementInputModel req)
        {
            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
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
                //    if (_stockDbContext.InventoryRequirement.Any(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementCode == req.InventoryRequirementCode))
                //        throw new BadRequestException(GeneralCode.InternalError, "Mã yêu cầu đã tồn tại");
                //}

                // validate product duplicate
                //if (inventoryType == EnumInventoryType.Output && req.InventoryRequirementDetail.GroupBy(d => new { d.ProductId, d.DepartmentId, d.ProductionStepId }).Any(g => g.Count() > 1))
                //    throw DuplicateProduct.BadRequest();

                await ValidateInventoryRequirementConfig(req.Date.UnixToDateTime(), null);
                var inventoryRequirement = _mapper.Map<InventoryRequirementEntity>(req);
                inventoryRequirement.InventoryTypeId = (int)inventoryType;
                inventoryRequirement.CensorStatus = (int)EnumInventoryRequirementStatus.Waiting;

                _stockDbContext.InventoryRequirement.Add(inventoryRequirement);
                await _stockDbContext.SaveChangesAsync();

                // Tạo file
                foreach (var item in req.InventoryRequirementFile)
                {
                    // Tạo mới
                    var inventoryRequirementFile = _mapper.Map<InventoryRequirementFile>(item);
                    inventoryRequirementFile.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDbContext.InventoryRequirementFile.Add(inventoryRequirementFile);
                }

                // Validate product
                var productIds = req.InventoryRequirementDetail.Select(d => d.ProductId).Distinct().ToList();
                if (_stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).Count() != productIds.Count)
                    throw ProductsMustBeCreated.BadRequest();

                // Tạo detail
                foreach (var item in req.InventoryRequirementDetail)
                {
                    // Tạo mới
                    var inventoryRequirementDetail = _mapper.Map<InventoryRequirementDetail>(item);
                    inventoryRequirementDetail.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDbContext.InventoryRequirementDetail.Add(inventoryRequirementDetail);
                }
                await _stockDbContext.SaveChangesAsync();
                trans.Commit();


                await ctx.ConfirmCode();

                //await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                await CreatedLogBuilder(objectType)
                   .MessageResourceFormatDatas(req.InventoryRequirementCode)
                   .ObjectId(inventoryRequirement.InventoryRequirementId)
                   .JsonData(req)
                   .CreateLog();


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


        private async Task<IGenerateCodeContext> GenerateInventoryRequirementCode(EnumInventoryType inventoryType, EnumObjectType objectTypeId, InventoryRequirementInputModel req)
        {
            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(objectTypeId)
                .SetConfigData(0, req.Date)
                .TryValidateAndGenerateCode(_stockDbContext.InventoryRequirement, req.InventoryRequirementCode, (s, code) => s.InventoryTypeId == (int)inventoryType && s.InventoryRequirementCode == code);

            req.InventoryRequirementCode = code;
            return ctx;
        }

        public async Task<long> Update(EnumInventoryType inventoryType, long inventoryRequirementId, InventoryRequirementInputModel req)
        {
            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

                var inventoryRequirement = _stockDbContext.InventoryRequirement
                    .Include(r => r.InventoryRequirementDetail)
                    .Include(r => r.InventoryRequirementFile)
                    .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

                if (inventoryRequirement == null) throw InvRequestNotFound.BadRequest();

                if (req.UpdatedDatetimeUtc != inventoryRequirement.UpdatedDatetimeUtc.GetUnix())
                {
                    throw GeneralCode.DataIsOld.BadRequest();
                }

                if (string.IsNullOrEmpty(req.InventoryRequirementCode)) req.InventoryRequirementCode = inventoryRequirement.InventoryRequirementCode;

                //if (inventoryRequirement.InventoryRequirementCode != req.InventoryRequirementCode)
                //    throw ChangeInRequestCodeNotAllow.BadRequest();

                //if (inventoryRequirement.ProductionOrderId.HasValue)
                //    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được thay đổi phiếu yêu cầu từ sản xuất");

                // validate product duplicate
                //if (inventoryType == EnumInventoryType.Output && req.InventoryRequirementDetail.GroupBy(d => new { d.ProductId, d.DepartmentId, d.ProductionStepId }).Any(g => g.Count() > 1))
                //    throw DuplicateProduct.BadRequest();

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
                    _stockDbContext.InventoryRequirementFile.Add(inventoryRequirementFile);
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
                        _stockDbContext.InventoryRequirementDetail.Add(inventoryRequirementDetail);
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

                if (_stockDbContext.HasChanges())
                    inventoryRequirement.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _stockDbContext.SaveChangesAsync();
                trans.Commit();

                await UpdatedLogBuilder(objectType)
                 .MessageResourceFormatDatas(req.InventoryRequirementCode)
                 .ObjectId(inventoryRequirement.InventoryRequirementId)
                 .JsonData(req)
                 .CreateLog();

                return inventoryRequirementId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateInventoryRequirement");
                throw;
            }
        }

        public async Task<bool> Delete(EnumInventoryType inventoryType, long inventoryRequirementId)
        {
            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

                var inventoryRequirement = _stockDbContext.InventoryRequirement
                    .Include(r => r.InventoryRequirementDetail)
                    .Include(r => r.InventoryRequirementFile)
                    .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

                if (inventoryRequirement == null) throw InvRequestNotFound.BadRequest();

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

                await _stockDbContext.SaveChangesAsync();
                trans.Commit();


                await DeletedLogBuilder(objectType)
                 .MessageResourceFormatDatas(inventoryRequirement.InventoryRequirementCode)
                 .ObjectId(inventoryRequirement.InventoryRequirementId)
                 .JsonData(inventoryRequirement)
                 .CreateLog();


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

        public async Task<bool> Confirm(EnumInventoryType inventoryType, long inventoryRequirementId, EnumInventoryRequirementStatus status, Dictionary<long, int> assignStocks = null)
        {
            var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.RequestInventoryInput : EnumObjectType.RequestInventoryOutput;

            var inventoryRequirement = _stockDbContext.InventoryRequirement
                .Include(r => r.InventoryRequirementDetail)
                .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

            if (inventoryRequirement == null) throw InvRequestNotFound.BadRequest();

            if (status == EnumInventoryRequirementStatus.Accepted && inventoryRequirement.CensorStatus == (int)EnumInventoryRequirementStatus.Accepted)
                throw InvRequestIsApprovedBefore.BadRequest();

            if (status == EnumInventoryRequirementStatus.Rejected && inventoryRequirement.CensorStatus != (int)EnumInventoryRequirementStatus.Waiting)
                throw InvRequestIsNotWaitingCensor.BadRequest();

            // Validate assign stock
            //if (status == EnumInventoryRequirementStatus.Accepted && (assignStocks == null || inventoryRequirement.InventoryRequirementDetail.Any(d => !assignStocks.ContainsKey(d.InventoryRequirementDetailId))))
            //    throw InvRequestDetailNeedAssignToStock.BadRequest();

            await ValidateInventoryRequirementConfig(inventoryRequirement.Date, inventoryRequirement.Date);

            inventoryRequirement.CensorStatus = (int)status;
            inventoryRequirement.CensorByUserId = _currentContextService.UserId;
            inventoryRequirement.CensorDatetimeUtc = DateTime.UtcNow;
            if (status == EnumInventoryRequirementStatus.Accepted)
            {
                foreach (var item in inventoryRequirement.InventoryRequirementDetail)
                {
                    if (assignStocks.ContainsKey(item.InventoryRequirementDetailId))
                    {
                        item.AssignStockId = assignStocks[item.InventoryRequirementDetailId];
                    }
                }
            }
            await _stockDbContext.SaveChangesAsync();
            return true;
        }


        private async Task ValidateInventoryRequirementConfig(DateTime? billDate, DateTime? oldDate)
        {
            await ValidateDateOfBill(billDate, oldDate);
        }

        public async Task<IList<InventoryRequirementOutputModel>> GetByProductionOrder(EnumInventoryType inventoryType, string productionOrderCode, EnumInventoryRequirementType? requirementType, int? productMaterialsConsumptionGroupId, long? productionOrderMaterialSetId)
        {
            var query = _stockDbContext.InventoryRequirement
                .Include(r => r.InventoryRequirementFile)
                .Include(r => r.InventoryRequirementDetail)
                .ThenInclude(d => d.ProductUnitConversion)
                .Where(r => r.InventoryTypeId == (int)inventoryType
                    && r.InventoryRequirementDetail.Any(rd => rd.ProductionOrderCode == productionOrderCode)
                    && (!requirementType.HasValue || r.InventoryRequirementTypeId == (int)requirementType.Value)
                    );

            if (productMaterialsConsumptionGroupId.HasValue)
            {
                query = query.Where(x => x.ProductMaterialsConsumptionGroupId == productMaterialsConsumptionGroupId);
            }

            if (productionOrderMaterialSetId.HasValue)
            {
                query = query.Where(x => x.ProductionOrderMaterialSetId == productionOrderMaterialSetId);
            }
            var lst = await query.ToListAsync();

            return lst.Select(r => _mapper.Map<InventoryRequirementOutputModel>(r)).ToList();

        }


        private ObjectActivityLogModelBuilder<string> CreatedLogBuilder(EnumObjectType objectType)
        {
            return objectType == EnumObjectType.RequestInventoryInput ?
                _invRequestActivityLog.LogBuilder(() => InventoryRequirementActivityLogMessage.RequireInputCreate)
                .ObjectType(EnumObjectType.RequestInventoryInput)
                :
                _invRequestActivityLog.LogBuilder(() => InventoryRequirementActivityLogMessage.RequireOutputCreate)
                .ObjectType(EnumObjectType.RequestInventoryOutput);
        }

        private ObjectActivityLogModelBuilder<string> UpdatedLogBuilder(EnumObjectType objectType)
        {
            return objectType == EnumObjectType.RequestInventoryInput ?
                _invRequestActivityLog.LogBuilder(() => InventoryRequirementActivityLogMessage.RequireInputUpdate)
                .ObjectType(EnumObjectType.RequestInventoryInput)
                :
                _invRequestActivityLog.LogBuilder(() => InventoryRequirementActivityLogMessage.RequireInputUpdate)
                .ObjectType(EnumObjectType.RequestInventoryOutput);
        }

        private ObjectActivityLogModelBuilder<string> DeletedLogBuilder(EnumObjectType objectType)
        {
            return objectType == EnumObjectType.RequestInventoryInput ?
                _invRequestActivityLog.LogBuilder(() => InventoryRequirementActivityLogMessage.RequireInputDelete)
                .ObjectType(EnumObjectType.RequestInventoryInput)
                :
                _invRequestActivityLog.LogBuilder(() => InventoryRequirementActivityLogMessage.RequireInputDelete)
                .ObjectType(EnumObjectType.RequestInventoryOutput);
        }
    }
}
