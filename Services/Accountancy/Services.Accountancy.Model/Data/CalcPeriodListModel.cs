using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Data
{
    public class CalcPeriodListModel
    {
        public long CalcPeriodId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilterHash { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
    }

    public class CalcPeriodDetailModel : CalcPeriodListModel
    {
        public string FilterData { get; set; }
        public string Data { get; set; }
    }

    public interface IFilterHashData
    {
        string GetHashString();
    }
}
