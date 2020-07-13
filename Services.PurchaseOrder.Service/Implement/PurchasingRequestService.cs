
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Model.Activity;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;
using VErp.Services.PurchaseOrder.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using System.IO;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchasingRequestService : IPurchasingRequestService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IProductHelperService _productHelperService;
        private readonly IMapper _mapper;

        public PurchasingRequestService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingRequestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
            , IProductHelperService productHelperService
            , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _productHelperService = productHelperService;
            _mapper = mapper;
        }


        public async Task<ServiceResult<PurchasingRequestOutput>> GetInfo(long purchasingRequestId)
        {
            var info = await _purchaseOrderDBContext
                .PurchasingRequest
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchasingRequestId == purchasingRequestId);

            if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

            var details = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking()
                .Where(d => d.PurchasingRequestId == purchasingRequestId)
                .ToListAsync();

            var data = _mapper.Map<PurchasingRequestOutput>(info);

            data.Details = details.Select(d => _mapper.Map<PurchasingRequestOutputDetail>(d)).ToList();

            return data;
        }


        public async Task<PageData<PurchasingRequestOutputList>> GetList(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = _purchaseOrderDBContext.PurchasingRequest.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchasingRequestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (productIds != null && productIds.Count > 0)
            {
                var purchasingRequestIds = (
                    from d in _purchaseOrderDBContext.PurchasingRequestDetail
                    where productIds.Contains(d.ProductId)
                    select d.PurchasingRequestId
                 ).Distinct();

                query = query.Where(q => purchasingRequestIds.Contains(q.PurchasingRequestId));
            }

            if (purchasingRequestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingRequestStatusId == (int)purchasingRequestStatusId.Value);
            }

            if (poProcessStatusId.HasValue)
            {
                query = query.Where(q => q.PoProcessStatusId == (int)poProcessStatusId.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(q => q.IsApproved == isApproved);
            }

            if (fromDate.HasValue)
            {
                var time = fromDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc >= time);
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc <= time);
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = new List<PurchasingRequestOutputList>();
            foreach (var info in pagedData)
            {
                result.Add(_mapper.Map<PurchasingRequestOutputList>(info));
            }

            return (result, total);

        }


        public async Task<PageData<PurchasingRequestOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {

            var query = from r in _purchaseOrderDBContext.PurchasingRequest
                        join d in _purchaseOrderDBContext.PurchasingRequestDetail on r.PurchasingRequestId equals d.PurchasingRequestId
                        select new
                        {
                            r.PurchasingRequestId,
                            r.PurchasingRequestStatusId,
                            r.Date,
                            d.OrderCode,
                            d.ProductionOrderCode,
                            r.PurchasingRequestCode,
                            r.Content,
                            r.PoProcessStatusId,
                            r.IsApproved,
                            r.CreatedDatetimeUtc,
                            r.CreatedByUserId,
                            r.UpdatedByUserId,
                            r.UpdatedDatetimeUtc,
                            r.CensorByUserId,
                            r.CensorDatetimeUtc,
                            d.PurchasingRequestDetailId,
                            d.ProductId,
                            d.PrimaryQuantity,
                            d.ProductUnitConversionId,
                            d.ProductUnitConversionQuantity,
                            d.Description
                        };

            if (productIds != null && productIds.Count > 0)
            {
                query = query.Where(q => productIds.Contains(q.ProductId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.ProductionOrderCode.Contains(keyword)
                    || q.PurchasingRequestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (purchasingRequestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingRequestStatusId == (int)purchasingRequestStatusId.Value);
            }

            if (poProcessStatusId.HasValue)
            {
                query = query.Where(q => q.PoProcessStatusId == (int)poProcessStatusId.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(q => q.IsApproved == isApproved);
            }

            if (fromDate.HasValue)
            {
                var time = fromDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc >= time);
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc <= time);
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = new List<PurchasingRequestOutputListByProduct>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchasingRequestOutputListByProduct()
                {
                    PurchasingRequestId = info.PurchasingRequestId,
                    PurchasingRequestCode = info.PurchasingRequestCode,
                    Date = info.Date.GetUnix(),
                    OrderCode = info.OrderCode,
                    ProductionOrderCode = info.ProductionOrderCode,
                    PurchasingRequestStatusId = (EnumPurchasingRequestStatus)info.PurchasingRequestStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc?.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),

                    PurchasingRequestDetailId = info.PurchasingRequestDetailId,
                    ProductId = info.ProductId,
                    PrimaryQuantity = info.PrimaryQuantity,


                    ProductUnitConversionId = info.ProductUnitConversionId,
                    ProductUnitConversionQuantity = info.ProductUnitConversionQuantity,

                    Description = info.Description
                });
            }

            return (result, total);

        }


        public async Task<IList<PurchasingRequestDetailInfo>> PurchasingRequestDetailInfo(IList<long> purchasingRequestDetailIds)
        {
            if (purchasingRequestDetailIds == null || purchasingRequestDetailIds.Count == 0)
                return new List<PurchasingRequestDetailInfo>();

            return await (
                from d in _purchaseOrderDBContext.PurchasingRequestDetail
                join r in _purchaseOrderDBContext.PurchasingRequest on d.PurchasingRequestId equals r.PurchasingRequestId
                where purchasingRequestDetailIds.Contains(d.PurchasingRequestDetailId)
                select new PurchasingRequestDetailInfo
                {
                    PurchasingRequestId = r.PurchasingRequestId,
                    PurchasingRequestCode = r.PurchasingRequestCode,
                    PurchasingRequestDetailId = d.PurchasingRequestDetailId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    ProductUnitConversionId = d.ProductUnitConversionId,
                    ProductUnitConversionQuantity = d.ProductUnitConversionQuantity
                })
            .ToListAsync();
        }



        public async Task<long> Create(PurchasingRequestInput model)
        {
            await ValidateProductUnitConversion(model);

            model.PurchasingRequestCode = (model.PurchasingRequestCode ?? "").Trim();

            if (!string.IsNullOrEmpty(model.PurchasingRequestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.PurchasingRequestCode == model.PurchasingRequestCode);
                if (existedItem != null) throw new BadRequestException(PurchasingRequestErrorCode.RequestCodeAlreadyExisted);
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var purchasingRequest = _mapper.Map<PurchasingRequest>(model);

                purchasingRequest.RejectCount = 0;
                purchasingRequest.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Draff;
                purchasingRequest.IsApproved = null;
                purchasingRequest.PoProcessStatusId = null;
                purchasingRequest.IsDeleted = false;
                purchasingRequest.CreatedByUserId = _currentContext.UserId;
                purchasingRequest.UpdatedByUserId = _currentContext.UserId;
                purchasingRequest.CreatedDatetimeUtc = DateTime.UtcNow;
                purchasingRequest.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.AddAsync(purchasingRequest);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var purchasingRequestDetailList = model.Details.Select(d => _mapper.Map<PurchasingRequestDetail>(d)).ToList();

                foreach (var item in purchasingRequestDetailList)
                {
                    item.PurchasingRequestId = purchasingRequest.PurchasingRequestId;

                    item.CreatedDatetimeUtc = DateTime.UtcNow;
                    item.UpdatedDatetimeUtc = DateTime.UtcNow;
                    item.IsDeleted = false;
                    item.DeletedDatetimeUtc = null;
                }

                await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequest.PurchasingRequestId, $"Thêm mới phiếu yêu cầu VTHH  {purchasingRequest.PurchasingRequestCode}", model.JsonSerialize());

                return purchasingRequest.PurchasingRequestId;
            }
        }

        public async Task<Enum> Update(long purchasingRequestId, PurchasingRequestInput model)
        {
            await ValidateProductUnitConversion(model);

            model.PurchasingRequestCode = (model.PurchasingRequestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingRequestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.PurchasingRequestId != purchasingRequestId && r.PurchasingRequestCode == model.PurchasingRequestCode);
                if (existedItem != null) return PurchasingRequestErrorCode.RequestCodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                _mapper.Map(model, info);

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Draff;
                info.IsApproved = null;
                info.UpdatedByUserId = _currentContext.UserId;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(d => d.PurchasingRequestId == purchasingRequestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                var purchasingRequestDetailList = model.Details.Select(d => _mapper.Map<PurchasingRequestDetail>(d)).ToList();
                foreach (var item in purchasingRequestDetailList)
                {
                    item.PurchasingRequestId = purchasingRequestId;

                    item.CreatedDatetimeUtc = DateTime.UtcNow;
                    item.UpdatedDatetimeUtc = DateTime.UtcNow;
                    item.IsDeleted = false;
                    item.DeletedDatetimeUtc = null;
                }

                await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Cập nhật phiếu yêu cầu VTHH  {info.PurchasingRequestCode}", model.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Delete(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;


                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(d => d.PurchasingRequestId == purchasingRequestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Xóa phiếu yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async IAsyncEnumerable<PurchasingRequestInputDetail> ParseInvoiceDetails(SingleInvoicePurchasingRequestExcelMappingModel mapping, Stream stream)
        {
            var rowDatas = SingleInvoiceParseExcel(mapping, stream).ToList();

            var productCodes = rowDatas.Select(r => r.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInternalName).ToList();

            var productInfos = await _productHelperService.GetListByCodeAndInternalNames(productCodes, productInternalNames);

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                IList<ProductModel> productInfo = null;
                if (!string.IsNullOrWhiteSpace(item.ProductCode) && productInfoByCode.ContainsKey(item.ProductCode?.ToLower()))
                {
                    productInfo = productInfoByCode[item.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductInternalName) && productInfoByInternalName.ContainsKey(item.ProductInternalName))
                    {
                        productInfo = productInfoByInternalName[item.ProductInternalName];
                    }
                }

                if (productInfo == null || productInfo.Count == 0)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy mặt hàng {item.ProductCode} {item.ProductName}");
                }

                if (productInfo.Count > 1)
                {
                    productInfo = productInfo.Where(p => p.ProductName == item.ProductName).ToList();

                    if (productInfo.Count != 1)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy {productInfo.Count} mặt hàng {item.ProductCode} {item.ProductName}");
                }

                var productUnitConversionId = 0;
                if (!string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                {
                    var pus = productInfo[0].StockInfo.UnitConversions
                            .Where(u => u.ProductUnitConversionName.NormalizeAsInternalName() == item.ProductUnitConversionName.NormalizeAsInternalName())
                            .ToList();

                    if (pus.Count != 1)
                    {
                        pus = productInfo[0].StockInfo.UnitConversions
                           .Where(u => u.ProductUnitConversionName.Contains(item.ProductUnitConversionName) || item.ProductUnitConversionName.Contains(u.ProductUnitConversionName))
                           .ToList();

                        if (pus.Count > 1)
                        {
                            pus = productInfo[0].StockInfo.UnitConversions
                             .Where(u => u.ProductUnitConversionName.Equals(item.ProductUnitConversionName, StringComparison.OrdinalIgnoreCase))
                             .ToList();
                        }
                    }

                    if (pus.Count == 0)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị chuyển đổi {item.ProductUnitConversionName} mặt hàng {item.ProductCode} {item.ProductName}");
                    }
                    if (pus.Count > 1)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy {pus.Count} đơn vị chuyển đổi {item.ProductUnitConversionName} mặt hàng {item.ProductCode} {item.ProductName}");
                    }

                    productUnitConversionId = pus[0].ProductUnitConversionId;

                }
                else
                {
                    var puDefault = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);
                    if (puDefault == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Dữ liệu đơn vị tính default lỗi, mặt hàng {item.ProductCode} {item.ProductName}");

                    }
                    productUnitConversionId = puDefault.ProductUnitConversionId;
                }

                yield return new PurchasingRequestInputDetail()
                {
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    ProductId = productInfo[0].ProductId.Value,
                    PrimaryQuantity = item.PrimaryQuantity,
                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                };

            }
        }

        private IEnumerable<PurchasingRequestDetailRowValue> SingleInvoiceParseExcel(SingleInvoicePurchasingRequestExcelMappingModel mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();


            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];
                if (row.Count == 0) continue;

                var rowData = new PurchasingRequestDetailRowValue();

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductCodeColumn))
                {
                    rowData.ProductCode = row[mapping.ColumnMapping.ProductCodeColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductNameColumn))
                {
                    rowData.ProductName = row[mapping.ColumnMapping.ProductNameColumn]?.ToString();
                    rowData.ProductInternalName = rowData.ProductName.NormalizeAsInternalName();
                }

                if (string.IsNullOrWhiteSpace(rowData.ProductCode) && string.IsNullOrWhiteSpace(rowData.ProductName)) continue;


                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.PrimaryQuantityColumn)
                    && row[mapping.ColumnMapping.PrimaryQuantityColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.PrimaryQuantityColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.PrimaryQuantity = Convert.ToDecimal(row[mapping.ColumnMapping.PrimaryQuantityColumn]);                        
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }

                }


                if (!string.IsNullOrWhiteSpace(mapping.StaticValue.ProductUnitConversionName))
                {
                    rowData.ProductUnitConversionName = mapping.StaticValue.ProductUnitConversionName;
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductUnitConversionNameColumn))
                {
                    rowData.ProductUnitConversionName = row[mapping.ColumnMapping.ProductUnitConversionNameColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(rowData.ProductUnitConversionName)
                    && !string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductUnitConversionQuantityColumn)
                    && row[mapping.ColumnMapping.ProductUnitConversionQuantityColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.ProductUnitConversionQuantityColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.ProductUnitConversionQuantity = Convert.ToDecimal(row[mapping.ColumnMapping.ProductUnitConversionQuantityColumn]);                      
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng ĐVCĐ ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }
                }

                //if (rowData.ProductUnitConversionQuantity == 0)
                //{
                //    rowData.ProductUnitConversionName = null;
                //}

                if (rowData.PrimaryQuantity <= 0 && rowData.ProductUnitConversionQuantity <= 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng không hợp lệ ở mặt hàng {rowData.ProductCode} {rowData.ProductName}");
                }


                if (!string.IsNullOrWhiteSpace(mapping.StaticValue.OrderCode))
                {
                    rowData.OrderCode = mapping.StaticValue.OrderCode;
                }
                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.OrderCodeColumn))
                {
                    rowData.OrderCode = row[mapping.ColumnMapping.OrderCodeColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.StaticValue.ProductionOrderCode))
                {
                    rowData.ProductionOrderCode = mapping.StaticValue.ProductionOrderCode;
                }
                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductionOrderCodeColumn))
                {
                    rowData.ProductionOrderCode = row[mapping.ColumnMapping.ProductionOrderCodeColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.DescriptionColumn))
                {
                    rowData.Description = row[mapping.ColumnMapping.DescriptionColumn]?.ToString();
                }

                yield return rowData;
            }
        }

        public async Task<Enum> SendToCensor(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Draff)
                {
                    return GeneralCode.InvalidParams;
                }

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Gửi duyệt yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Approve(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                //allow re censored
                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.WaitToCensor
                    && info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Censored
                    )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = true;
                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Duyệt yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Reject(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;
                //allow re censored
                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.WaitToCensor
                    && info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Censored
                    )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = false;
                info.RejectCount++;

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Từ chối yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> UpdatePoProcessStatus(long purchasingRequestId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Cập nhật tiến trình PO yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        private async Task ValidateProductUnitConversion(PurchasingRequestInput model)
        {
            var productUnitConversionProductGroup = model.Details.Select(d => new { d.ProductUnitConversionId, d.ProductId })
               .GroupBy(d => d.ProductUnitConversionId);
            if (productUnitConversionProductGroup.Any(g => g.Select(p => p.ProductId).Distinct().Count() > 1))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Đơn vị chuyển đổi không thuộc về mặt hàng!");
            }

            if (!await _productHelperService.ValidateProductUnitConversions(productUnitConversionProductGroup.ToDictionary(g => g.Key, g => g.First().ProductId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Đơn vị chuyển đổi không thuộc về mặt hàng!");
            }
        }

    }
}
