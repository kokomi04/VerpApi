using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Activity;
using VErp.Services.Master.Service.Activity;
using VErp.Services.PurchaseOrder.Model.PurchasingSuggest;
using VErp.Services.PurchaseOrder.Service.PurchasingSuggest;

namespace Services.PurchaseOrder.Service.PurchasingSuggest.Implement
{
    public class PurchasingSuggestService : IPurchasingSuggestService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IActivityService _activityService;
        private readonly IAsyncRunnerService _asyncRunner;

        public PurchasingSuggestService(MasterDBContext masterDBContext, StockDBContext stockContext, PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
            , IActivityService activityService
           , IAsyncRunnerService asyncRunner
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _masterDBContext = masterDBContext;
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _activityService = activityService;
            _asyncRunner = asyncRunner;
        }

        public async Task<ServiceResult<PurchasingSuggestOutputModel>> Get(long purchasingSuggestId)
        {
            try
            {
                var psObj = _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking().FirstOrDefault(q => q.PurchasingSuggestId == purchasingSuggestId);
                if (psObj == null)
                {
                    return PurchasingSuggestErrorCode.NotFound;
                }
                var psDetailsDataList = _purchaseOrderDBContext.PurchasingSuggestDetail.AsNoTracking().Where(q => q.PurchasingSuggestId == psObj.PurchasingSuggestId).ToList();

                var productIdList = psDetailsDataList.Select(q => q.ProductId).ToList();
                var productDataList = await _stockDbContext.Product.Where(q => productIdList.Contains(q.ProductId)).AsNoTracking().Select(q => new { q.ProductId, q.ProductCode, q.ProductName, q.UnitId }).ToListAsync();

                var unitIdList = psDetailsDataList.Select(q => q.PrimaryUnitId).ToList();
                var unitDataList = await _masterDBContext.Unit.Where(q => unitIdList.Contains(q.UnitId)).AsNoTracking().ToListAsync();

                var customerIdList = psDetailsDataList.Select(q => q.PrimaryUnitId).ToList();
                var customerDataList = await _masterDBContext.Customer.Where(q => customerIdList.Contains(q.CustomerId)).AsNoTracking().Select(q => new
                {
                    q.CustomerId,
                    q.CustomerCode,
                    q.CustomerName
                }).ToListAsync();

                var detailsModelOutput = psDetailsDataList.Select(q => new PurchasingSuggestDetailOutputModel
                {
                    PurchasingSuggestDetailId = q.PurchasingSuggestDetailId,
                    PurchasingSuggestId = q.PurchasingSuggestId,
                    PurchasingRequestCode = q.PurchasingRequestCode,
                    CustomerId = q.CustomerId ?? 0,
                    CustomerCode = q.CustomerId > 0 ? customerDataList.FirstOrDefault(v => v.CustomerId == q.CustomerId)?.CustomerCode : string.Empty,
                    CustomerName = q.CustomerId > 0 ? customerDataList.FirstOrDefault(v => v.CustomerId == q.CustomerId)?.CustomerName : string.Empty,
                    ProductId = q.ProductId,
                    ProductCode = productDataList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductCode,
                    ProductName = productDataList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductName,
                    PrimaryUnitId = q.PrimaryUnitId,
                    PrimaryUnitName = unitDataList.FirstOrDefault(v => v.UnitId == q.PrimaryUnitId)?.UnitName,
                    PrimaryQuantity = q.PrimaryQuantity,
                    PrimaryUnitPrice = q.PrimaryUnitPrice ?? 0,
                    Tax = q.Tax ?? 0
                }).ToList();

                var result = new PurchasingSuggestOutputModel
                {
                    PurchasingSuggestId = psObj.PurchasingSuggestId,
                    PurchasingSuggestCode = psObj.PurchasingSuggestCode,
                    OrderCode = psObj.OrderCode,
                    Date = psObj.Date.GetUnix(),
                    Content = psObj.Content,
                    Status = psObj.Status,
                    RejectCount = psObj.RejectCount,
                    CreatedByUserId = psObj.CreatedByUserId,
                    UpdatedByUserId = psObj.UpdatedByUserId,
                    CreatedDatetimeUtc = psObj.CreatedDatetimeUtc != null ? ((DateTime)psObj.CreatedDatetimeUtc).GetUnix() : 0,
                    UpdatedDatetimeUtc = psObj.UpdatedDatetimeUtc != null ? ((DateTime)psObj.UpdatedDatetimeUtc).GetUnix() : 0,
                    CensorByUserId = psObj.CensorByUserId,
                    CensorDatetimeUtc = psObj.CensorDatetimeUtc != null ? ((DateTime)psObj.CensorDatetimeUtc).GetUnix() : 0,

                    DetailList = detailsModelOutput
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get PurchasingSuggest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<PageData<PurchasingSuggestOutputModel>> GetList(string keyword, IList<int> statusList, long beginTime = 0, long endTime = 0, int page = 1, int size = 10)
        {
            var psQuery = from pr in _purchaseOrderDBContext.PurchasingSuggest
                          select pr;

            if (!string.IsNullOrEmpty(keyword))
            {
                psQuery = psQuery.Where(q => q.PurchasingSuggestCode.Contains(keyword) || q.OrderCode.Contains(keyword));
            }
            if (statusList.Count > 0)
            {
                var enumStatusList = statusList.Select(q => Convert.ToInt32(q)).ToList();
                psQuery = psQuery.Where(q => enumStatusList.Contains(q.Status));
            }
            var bTime = DateTime.MinValue;
            var eTime = DateTime.MinValue;

            if (beginTime > 0)
            {
                bTime = beginTime.UnixToDateTime();
            }
            if (endTime > 0)
            {
                eTime = endTime.UnixToDateTime();
                eTime = eTime.AddDays(1);
            }

            if (bTime != DateTime.MinValue && eTime != DateTime.MinValue)
            {
                psQuery = psQuery.Where(q => q.Date >= bTime && q.Date < eTime);
            }
            else
            {
                if (bTime != DateTime.MinValue)
                {
                    psQuery = psQuery.Where(q => q.Date >= bTime);
                }
                if (eTime != DateTime.MinValue)
                {
                    psQuery = psQuery.Where(q => q.Date < eTime);
                }
            }

            psQuery = psQuery.OrderByDescending(q => q.Date);
            var total = psQuery.Count();
            var psDataList = psQuery.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
            var psIdList = psDataList.Select(q => q.PurchasingSuggestId).ToList();

            var psDetailsDataList = _purchaseOrderDBContext.PurchasingSuggestDetail.AsNoTracking().Where(q => psIdList.Contains(q.PurchasingSuggestId)).ToList();

            var productIdsList = psDetailsDataList.Select(q => q.ProductId).ToList();
            var productDataList = await _stockDbContext.Product.Where(q => productIdsList.Contains(q.ProductId)).AsNoTracking().Select(q => new { q.ProductId, q.ProductCode, q.ProductName, q.UnitId }).ToListAsync();

            var unitIdsList = psDetailsDataList.Select(q => q.PrimaryUnitId).ToList();
            var unitDataList = await _masterDBContext.Unit.Where(q => unitIdsList.Contains(q.UnitId)).AsNoTracking().ToListAsync();

            var customerIdList = psDetailsDataList.Select(q => q.PrimaryUnitId).ToList();
            var customerDataList = await _masterDBContext.Customer.Where(q => customerIdList.Contains(q.CustomerId)).AsNoTracking().Select(q => new
            {
                q.CustomerId,
                q.CustomerCode,
                q.CustomerName
            }).ToListAsync();

            var psDetailsModelOutputList = psDetailsDataList.Select(q => new PurchasingSuggestDetailOutputModel
            {
                PurchasingSuggestDetailId = q.PurchasingSuggestDetailId,
                PurchasingSuggestId = q.PurchasingSuggestId,
                PurchasingRequestCode = q.PurchasingRequestCode,
                CustomerId = q.CustomerId ?? 0,
                CustomerCode = q.CustomerId > 0 ? customerDataList.FirstOrDefault(v => v.CustomerId == q.CustomerId)?.CustomerCode : string.Empty,
                CustomerName = q.CustomerId > 0 ? customerDataList.FirstOrDefault(v => v.CustomerId == q.CustomerId)?.CustomerName : string.Empty,
                ProductId = q.ProductId,
                ProductCode = productDataList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductCode,
                ProductName = productDataList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductName,
                PrimaryUnitId = q.PrimaryUnitId,
                PrimaryUnitName = unitDataList.FirstOrDefault(v => v.UnitId == q.PrimaryUnitId)?.UnitName,
                PrimaryQuantity = q.PrimaryQuantity,
                PrimaryUnitPrice = q.PrimaryUnitPrice ?? 0,
                Tax = q.Tax ?? 0
            }).ToList();

            var pagedData = new List<PurchasingSuggestOutputModel>(psDataList.Count);
            foreach (var psItem in psDataList)
            {
                var psOutputModel = new PurchasingSuggestOutputModel
                {
                    PurchasingSuggestId = psItem.PurchasingSuggestId,
                    PurchasingSuggestCode = psItem.PurchasingSuggestCode,
                    OrderCode = psItem.OrderCode,
                    Date = psItem.Date.GetUnix(),
                    Content = psItem.Content,
                    Status = psItem.Status,
                    RejectCount = psItem.RejectCount,
                    CreatedByUserId = psItem.CreatedByUserId,
                    UpdatedByUserId = psItem.UpdatedByUserId,
                    CreatedDatetimeUtc = psItem.CreatedDatetimeUtc != null ? ((DateTime)psItem.CreatedDatetimeUtc).GetUnix() : 0,
                    UpdatedDatetimeUtc = psItem.UpdatedDatetimeUtc != null ? ((DateTime)psItem.UpdatedDatetimeUtc).GetUnix() : 0,
                    CensorByUserId = psItem.CensorByUserId,
                    CensorDatetimeUtc = psItem.CensorDatetimeUtc != null ? ((DateTime)psItem.CensorDatetimeUtc).GetUnix() : 0,

                    DetailList = psDetailsModelOutputList.Where(q => q.PurchasingSuggestId == psItem.PurchasingSuggestId).ToList()
                };
                pagedData.Add(psOutputModel);
            }
            return (pagedData, total);
        }

        public async Task<ServiceResult<long>> AddPurchasingSuggest(int currentUserId, PurchasingSuggestInputModel model)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasingSuggestObj = new VErp.Infrastructure.EF.PurchaseOrderDB.PurchasingSuggest()
                    {
                        PurchasingSuggestCode = model.PurchasingSuggestCode,
                        OrderCode = model.OrderCode,
                        Date = model.Date > 0 ? model.Date.UnixToDateTime() : DateTime.UtcNow,
                        Content = model.Content,
                        Status = (int)EnumPurchasingSuggestStatus.Editing,
                        RejectCount = 0,

                        CreatedByUserId = currentUserId,
                        UpdatedByUserId = currentUserId,

                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _purchaseOrderDBContext.AddAsync(purchasingSuggestObj);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var psDetailsList = new List<PurchasingSuggestDetail>(model.DetailList.Count);

                    foreach (var inputModel in model.DetailList)
                    {
                        psDetailsList.Add(new PurchasingSuggestDetail
                        {
                            PurchasingSuggestId = purchasingSuggestObj.PurchasingSuggestId,
                            CustomerId = inputModel.CustomerId,
                            PurchasingRequestCode = inputModel.PurchasingRequestCode,
                            ProductId = inputModel.ProductId,
                            PrimaryUnitId = inputModel.PrimaryUnitId,
                            PrimaryQuantity = inputModel.PrimaryQuantity,
                            PrimaryUnitPrice = inputModel.PrimaryUnitPrice,
                            Tax = inputModel.Tax,
                            IsDeleted = false,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                        });
                    }
                    await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(psDetailsList);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestObj.PurchasingSuggestId, $"Thêm mới phiếu đề nghị mua  {purchasingSuggestObj.PurchasingSuggestCode}", model.JsonSerialize());

                    return purchasingSuggestObj.PurchasingSuggestId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddPurchasingSuggest");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> UpdatePurchasingSuggest(long purchasingSuggestId, int currentUserId, PurchasingSuggestInputModel model)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasingSuggestObj = _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefault(q => q.PurchasingSuggestId == purchasingSuggestId);
                    if (purchasingSuggestObj == null)
                    {
                        return PurchasingSuggestErrorCode.NotFound;
                    }
                    //purchasingRequestObj.PurchasingRequestCode = model.PurchasingRequestCode;
                    purchasingSuggestObj.OrderCode = model.OrderCode;
                    purchasingSuggestObj.Date = model.Date.UnixToDateTime();
                    purchasingSuggestObj.Content = model.Content;
                    purchasingSuggestObj.UpdatedByUserId = currentUserId;
                    purchasingSuggestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var oldPsDetailsList = _purchaseOrderDBContext.PurchasingSuggestDetail.Where(q => q.PurchasingSuggestId == purchasingSuggestObj.PurchasingSuggestId).ToList();

                    foreach (var details in oldPsDetailsList)
                    {
                        details.IsDeleted = true;
                        details.UpdatedDatetimeUtc = DateTime.UtcNow;
                    }
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var newPsDetailsList = new List<PurchasingSuggestDetail>(model.DetailList.Count);

                    foreach (var inputModel in model.DetailList)
                    {
                        newPsDetailsList.Add(new PurchasingSuggestDetail
                        {
                            PurchasingSuggestId = purchasingSuggestObj.PurchasingSuggestId,
                            CustomerId = inputModel.CustomerId,
                            PurchasingRequestCode = inputModel.PurchasingRequestCode,
                            ProductId = inputModel.ProductId,
                            PrimaryUnitId = inputModel.PrimaryUnitId,
                            PrimaryQuantity = inputModel.PrimaryQuantity,
                            PrimaryUnitPrice = inputModel.PrimaryUnitPrice,
                            Tax = inputModel.Tax,
                            IsDeleted = false,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                        });
                    }
                    await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(newPsDetailsList);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestObj.PurchasingSuggestId, $"Cập nhật phiếu đề nghị mua VTHH  {purchasingSuggestObj.PurchasingSuggestCode}", purchasingSuggestObj.JsonSerialize());

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UpdatePurchasingSuggest");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeletePurchasingSuggest(long purchasingSuggestId, int currentUserId)
        {
            try
            {
                var purchasingSuggestObj = _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefault(q => q.PurchasingSuggestId == purchasingSuggestId);
                if (purchasingSuggestObj == null)
                {
                    return PurchasingSuggestErrorCode.NotFound;
                }
                if (purchasingSuggestObj.Status == (int)EnumPurchasingRequestStatus.Approved)
                {
                    return PurchasingSuggestErrorCode.AlreadyApproved;
                }
                purchasingSuggestObj.IsDeleted = true;
                purchasingSuggestObj.UpdatedByUserId = currentUserId;
                purchasingSuggestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestObj.PurchasingSuggestId, $"Xóa phiếu đề nghị mua VTHH  {purchasingSuggestObj.PurchasingSuggestCode}", purchasingSuggestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeletePurchasingSuggest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> SendToApprove(long purchasingSuggestId, int currentUserId)
        {
            try
            {
                var purchasingSuggestObj = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(q => q.PurchasingSuggestId == purchasingSuggestId);
                if (purchasingSuggestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                purchasingSuggestObj.Status = (int)EnumPurchasingSuggestStatus.WaitingApproved;
                purchasingSuggestObj.UpdatedByUserId = currentUserId;
                purchasingSuggestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestObj.PurchasingSuggestId, $"Gửi duyệt phiếu đề nghị mua VTHH  {purchasingSuggestObj.PurchasingSuggestCode}", purchasingSuggestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendToApprove");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> ApprovePurchasingSuggest(long purchasingSuggestId, int currentUserId)
        {
            try
            {
                var purchasingSuggestObj = _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefault(q => q.PurchasingSuggestId == purchasingSuggestId);
                if (purchasingSuggestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                purchasingSuggestObj.Status = (int)EnumPurchasingSuggestStatus.Approved;
                purchasingSuggestObj.UpdatedByUserId = currentUserId;
                purchasingSuggestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestObj.PurchasingSuggestId, $"Duyệt phiếu đề nghị mua VTHH  {purchasingSuggestObj.PurchasingSuggestCode}", purchasingSuggestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApprovePurchasingRequest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> RejectPurchasingSuggest(long purchasingSuggestId, int currentUserId)
        {
            try
            {
                var purchasingSuggestObj = _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefault(q => q.PurchasingSuggestId == purchasingSuggestId);
                if (purchasingSuggestObj == null)
                {
                    return PurchasingSuggestErrorCode.NotFound;
                }
                if (purchasingSuggestObj.IsDeleted || purchasingSuggestObj.Status == (int)EnumPurchasingSuggestStatus.Approved)
                {
                    return PurchasingSuggestErrorCode.AlreadyApproved;
                }


                purchasingSuggestObj.Status = (int)EnumPurchasingSuggestStatus.Rejected;
                purchasingSuggestObj.RejectCount += 1;
                purchasingSuggestObj.CensorByUserId = currentUserId;
                purchasingSuggestObj.CensorDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestObj.PurchasingSuggestId, $"Từ chối phiếu đề nghị mua VTHH  {purchasingSuggestObj.PurchasingSuggestCode}", purchasingSuggestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RejectPurchasingSuggest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> AddNote(long objectId, int currentUserId, int actionTypeId = 0, string note = "")
        {
            var otId = (int)EnumObjectType.PurchasingSuggest;
            if (actionTypeId == 0)
                actionTypeId = 1;
            return await _activityService.CreateUserActivityLog(objectId: objectId, objectTypeId: otId, userId: currentUserId, actionTypeId: actionTypeId, message: note);
        }

        public async Task<PageData<UserActivityLogOuputModel>> GetNoteList(long objectId, int pageIndex = 1, int pageSize = 20)
        {
            var otId = (int)EnumObjectType.PurchasingSuggest;
            return await _activityService.GetListUserActivityLog(objectId: objectId, objectTypeId: otId, pageIndex, pageSize);
        }
    }
}
