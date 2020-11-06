using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.PackingList;
using PackingListEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PackingList;
using VErp.Commons.Library;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Constants;
using VErp.Infrastructure.EF.EFExtensions;
using Microsoft.Data.SqlClient;
using VErp.Commons.Enums.StandardEnum;
using OpenXmlPowerTools;

namespace VErp.Services.PurchaseOrder.Service.PackingList.Implement
{
    public class PackingListService : IPackingListService
    {
        public readonly string PACKINGLISTPRODUCTINVOUCHERBILL_VIEW = PurchaseOrderConstants.PACKINGLISTPRODUCTINVOUCHERBILL_VIEW;

        private readonly PurchaseOrderDBContext _purchaseOrderDB;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public PackingListService(PurchaseOrderDBContext purchaseOrderDB
            , ILogger<PackingListService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService)
        {
            _purchaseOrderDB = purchaseOrderDB;
            _mapper = mapper;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
        }

        public async Task<int> CreatePackingList(long voucherBillId, PackingListModel req)
        {
            await InvalidPackingList(voucherBillId, req);
            using (var trans = _purchaseOrderDB.Database.BeginTransaction())
            {
                try
                {
                    var pl = _mapper.Map<PackingListEntity>(req);

                    await _purchaseOrderDB.PackingList.AddAsync(pl);
                    await _purchaseOrderDB.SaveChangesAsync();

                    var details = _mapper.Map<List<PackingListDetail>>(req.Details);
                    details.ForEach(d => d.PackingListId = pl.PackingListId);

                    await _purchaseOrderDB.PackingListDetail.AddRangeAsync(details);
                    await _purchaseOrderDB.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.PackingList, pl.PackingListId, $"Tạo packinglist cho chứng từ bán hàng {pl.VoucherBillId}", pl.JsonSerialize());

                    return pl.PackingListId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw new BadRequestException(PackingListErrorCode.CanNotCreate, ex.Message);
                }
            }
        }

        public async Task<bool> DeletePackingList(int packingListId)
        {
            var info = await _purchaseOrderDB.PackingList.FirstOrDefaultAsync(p => p.PackingListId == packingListId);
            if (info == null)
                throw new BadRequestException(PackingListErrorCode.NotFoundPackingList);

            var details = _purchaseOrderDB.PackingListDetail.Where(x => x.PackingListId == info.PackingListId).ToList();

            details.ForEach(x => x.IsDeleted = true);
            info.IsDeleted = true;

            await _purchaseOrderDB.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.PackingList, info.PackingListId, $"Delete packinglist cho chứng từ bán hàng {info.VoucherBillId}", info.JsonSerialize());
            return true;
        }

        public async Task<PackingListModel> GetPackingListById(int packingListId)
        {
            var info = await _purchaseOrderDB.PackingList
                .Where(p => p.PackingListId == packingListId)
                .ProjectTo<PackingListModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (info == null)
                throw new BadRequestException(PackingListErrorCode.NotFoundPackingList);

            return info;
        }

        public async Task<PageData<PackingListModel>> GetPackingLists(long voucherBillId, string keyword, int page, int size)
        {
            var query = _purchaseOrderDB.PackingList.AsNoTracking().Where(p => p.VoucherBillId == voucherBillId);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.ContSealNo.Contains(keyword) || p.PackingNote.Contains(keyword));
            }
            var total = await query.CountAsync();
            var data = query.Skip((page - 1) * size)
                            .Take(size)
                            .ProjectTo<PackingListModel>(_mapper.ConfigurationProvider)
                            .ToList();

            return (data, total);
        }

        public async Task<bool> UpdatePackingList(long voucherBillId, int packingListId, PackingListModel req)
        {
            var info = await _purchaseOrderDB.PackingList.FirstOrDefaultAsync(p => p.PackingListId == packingListId);
            if (info == null)
                throw new BadRequestException(PackingListErrorCode.NotFoundPackingList);
            var details = _purchaseOrderDB.PackingListDetail.Where(x => x.PackingListId == info.PackingListId).ToList();

            var old = _mapper.Map<PackingListModel>(info);
            old.Details = _mapper.Map<List<PackingListDetailModel>>(details);

            await InvalidPackingList(voucherBillId, req, old);
            using (var trans = _purchaseOrderDB.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map(req, info);
                    await _purchaseOrderDB.SaveChangesAsync();

                    var nDetail = req.Details.Where(x => !details.Select(x=>x.PackingListDetailId).Contains(x.PackingListDetailId)).ToList();
                    var uDetail = req.Details.Where(x => details.Select(x => x.PackingListDetailId).Contains(x.PackingListDetailId)).ToList();
                    var dDetail = details.Where(x => !req.Details.Select(x => x.PackingListDetailId).Contains(x.PackingListDetailId)).ToList();

                    //Update packinglist detail
                    foreach(var d in details)
                    {
                        var n = uDetail.FirstOrDefault(x => x.PackingListDetailId == d.PackingListDetailId);
                        _mapper.Map(n, d);
                    }
                    // Create packinglist detail
                    await _purchaseOrderDB.PackingListDetail.AddRangeAsync(_mapper.Map<List<PackingListDetail>>(nDetail));
                    // Delete packinglist detail
                    dDetail.ForEach(x => x.IsDeleted = true);

                    await _purchaseOrderDB.SaveChangesAsync();
                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.PackingList, req.PackingListId, $"Update packinglist cho chứng từ bán hàng {req.VoucherBillId}", req.JsonSerialize());

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw new BadRequestException(PackingListErrorCode.CanNotCreate, ex.Message);
                }
            }
        }

        public async Task<List<NonCamelCaseDictionary>> GetPackingListProductInVoucherBill(long voucherBillId)
        {
            return await GetProductInVoucherBill(voucherBillId);
        }

        #region private
        private async Task InvalidPackingList(long voucherBillId, PackingListModel req, PackingListModel old = null)
        {
            var dataQuery = await GetProductInVoucherBill(voucherBillId);

            foreach (var p in req.Details)
            {
                var piv = dataQuery.FirstOrDefault(x => (long)x["VoucherValueRowId"] == p.VoucherValueRowId);
                long rNumber, oldActualNumber;
                oldActualNumber = 0;

                if (old != null)
                {
                    var pOld = old.Details.FirstOrDefault(x => x.PackingListDetailId == p.PackingListDetailId);
                    if(pOld !=null)
                        oldActualNumber = pOld.ActualNumber;
                }

                rNumber = Convert.ToInt64(piv["RestNumber"]?? 0) + oldActualNumber - p.ActualNumber;

                if (rNumber < 0)
                    throw new BadRequestException(PackingListErrorCode.InvalidPackingList,
                        $"Số lượng của {piv["ProductCode"]} trong Cont {req.ContSealNo} bị vượt quá so với ban đầu");
            }
        }

        private async Task<List<NonCamelCaseDictionary>> GetProductInVoucherBill(long voucherBilldId)
        {
            var dataSql = $@"   Select v.* 
                                From {PACKINGLISTPRODUCTINVOUCHERBILL_VIEW} v
                                Where v.VoucherBillId = {voucherBilldId}";
            var dataQuery = (await _purchaseOrderDB.QueryDataTable(dataSql, Array.Empty<SqlParameter>())).ConvertData();
            return dataQuery;
        }
        #endregion
    }
}
