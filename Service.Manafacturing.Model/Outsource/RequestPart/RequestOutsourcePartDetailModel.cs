using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartDetailModel : IMapFrom<RequestOutsourcePartDetail>
    {
        public int RequestOutsourcePartDetailId { get; set; }
        public int ProductId { get; set; }
        public int Quanity { get; set; }
        public int UnitId { get; set; }
        public OutsourcePartProcessType Status { get; set; }
    }

    public class RequestOutsourcePartDetailInfo: RequestOutsourcePartDetailModel
    {
        public int RequestOutsourcePartId { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public long CreateDateRequest { get; set; }
        public long DateRequiredComplete { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string ProductPartName { get; set; }
        public string ProductPartCode { get; set; }
        public string OrderCode { get; set; }

    }


}
