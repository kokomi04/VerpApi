using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderPartInfo: OutsourceOrderModel
    {
        public string RequestOutsourcePartCode { get; set; }
        public long DateRequiredComplete { get; set; }
        public IList<OutsourceOrderPartDetailInfo> OutsourceOrderPartDetails { get; set; }
    }
}
