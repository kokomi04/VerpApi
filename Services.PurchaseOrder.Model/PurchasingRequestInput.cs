using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestInput : IMapFrom<PurchasingRequest>
    {
        public string PurchasingRequestCode { get; set; }

        public long Date { get; set; }
        public string Content { get; set; }
        public List<PurchasingRequestInputDetail> Details { set; get; }

        public void Mapping(Profile profile) => profile.CreateMap<PurchasingRequest, PurchasingRequestInput>()
           .ForMember(m => m.Details, m => m.Ignore())
           .ReverseMap()
           .ForMember(m => m.PurchasingRequestDetail, m => m.Ignore())
           .ForMember(m => m.Date, m => m.MapFrom(v => v.Date.UnixToDateTime()));
    }

    public class PurchasingRequestInputDetail : IMapFrom<PurchasingRequestDetail>
    {
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<PurchasingRequestDetail, PurchasingRequestInputDetail>()
          .ReverseMap()
          .ForMember(m => m.PurchasingRequest, m => m.Ignore())
          .ForMember(m => m.PurchasingSuggestDetail, m => m.Ignore());

    }
}
