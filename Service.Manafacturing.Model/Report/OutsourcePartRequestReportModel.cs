﻿using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class OutsourcePartRequestReportModel : IMapFrom<OutsourcePartRequestDetailExtractInfo>
    {
        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long OutsourcePartRequestDetailId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public int ProductPartId { get; set; }
        public string ProductPartTitle { get; set; }
        public string UnitName { get; set; }
        public string UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityComplete { get; set; }
        public long OutsourcePartRequestDetailFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<OutsourcePartRequestDetailExtractInfo, OutsourcePartRequestReportModel>()
                .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.GetUnix()))
                .ReverseMapCustom()
                .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.UnixToDateTime()));
        }
    }
}
