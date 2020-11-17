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
        public long RequestOutsourcePartDetailId { get; set; }
        public long RequestOutsourcePartId { get; set; }
        public int ProductPartId { get; set; }
        public int Quantity { get; set; }
        public int UnitId { get; set; }
        public OutsourcePartProcessType Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RequestOutsourcePartDetail, RequestOutsourcePartDetailModel>()
                .ForMember(m => m.ProductPartId, v => v.MapFrom(m => m.ProductId))
                .ReverseMap()
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductPartId));
        }
    }

    public class RequestOutsourcePartDetailInfo: RequestOutsourcePartDetailModel
    {
        public string RequestOutsourcePartCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public long CreateDateRequest { get; set; }
        public long DateRequiredComplete { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string ProductPartName { get; set; }
        public string ProductPartCode { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public int ProductOrderDetailQuantity { get; set; }
        public string ProductTitle { get; set; }

    }


}
