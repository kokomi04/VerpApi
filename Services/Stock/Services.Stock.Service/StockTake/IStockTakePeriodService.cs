﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.StockTake;

namespace VErp.Services.Stock.Service.StockTake

{
    public interface IStockTakePeriodService
    {
        Task<PageData<StockTakePeriotListModel>> GetStockTakePeriods(string keyword, int page, int size, long fromDate, long toDate, int[] stockIds);

        Task<StockTakePeriotModel> CreateStockTakePeriod(StockTakePeriotModel model);

        Task<StockTakePeriotModel> GetStockTakePeriod(long stockTakePeriodId);

        Task<PageData<StockRemainQuantity>> GetUncheckedData(long stockTakePeriodId, string keyword, int page, int size);

        Task<StockTakePeriotModel> UpdateStockTakePeriod(long stockTakePeriodId, StockTakePeriotModel model);

        Task<bool> DeleteStockTakePeriod(long stockTakePeriodId);


        Task<IList<StockRemainQuantity>> CalcStockRemainQuantity(CalcStockRemainInputModel model);

        Task<StockTakeAcceptanceCertificateModel> GetStockTakeAcceptanceCertificate(long stockTakePeriodId);

        Task<StockTakeAcceptanceCertificateModel> UpdateStockTakeAcceptanceCertificate(long stockTakePeriodId, StockTakeAcceptanceCertificateModel model);

        Task<bool> ConfirmStockTakeAcceptanceCertificate(long stockTakePeriodId, ConfirmAcceptanceCertificateModel status);

        Task<bool> DeleteStockTakeAcceptanceCertificate(long stockTakePeriodId);
    }
}
