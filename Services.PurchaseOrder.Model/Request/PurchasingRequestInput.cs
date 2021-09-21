using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Verp.Resources.PurchaseOrder.PurchasingRequest;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;
namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestInput : IMapFrom<PurchasingRequest>
    {
        public long OrderDetailId { get; set; }
        public long? MaterialCalcId { get; set; }

        public string PurchasingRequestCode { get; set; }

        public long Date { get; set; }
        public string Content { get; set; }
        public long? ProductionOrderId { get; set; }

        public List<PurchasingRequestInputDetail> Details { set; get; }

        public void Mapping(Profile profile) => profile.CreateMap<PurchasingRequest, PurchasingRequestInput>()
           .ForMember(m => m.Details, m => m.Ignore())
           .ReverseMap()
           .ForMember(m => m.PurchasingRequestDetail, m => m.Ignore())
           .ForMember(m => m.Date, m => m.MapFrom(v => v.Date.UnixToDateTime()));
    }

    public class PurchasingRequestInputDetail : IMapFrom<PurchasingRequestDetail>
    {
        public PurchasingRequestInputDetail()
        {

        }
        public int ProductId { get; set; }
        [Required(ErrorMessageResourceType = typeof(PurchasingRequestMessage), ErrorMessageResourceName = nameof(PurchasingRequestMessage.QuantityRequired))]
        [GreaterThan(0, ErrorMessageResourceType = typeof(PurchasingRequestMessage), ErrorMessageResourceName = nameof(PurchasingRequestMessage.QuantityInvalid))]
        public decimal PrimaryQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        [Required(ErrorMessageResourceType = typeof(PurchasingRequestMessage), ErrorMessageResourceName = nameof(PurchasingRequestMessage.QuantityRequired))]
        [GreaterThan(0, ErrorMessageResourceType = typeof(PurchasingRequestMessage), ErrorMessageResourceName = nameof(PurchasingRequestMessage.QuantityInvalid))]
        public decimal ProductUnitConversionQuantity { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }

        public int OriginalProductId { get; set; }

        public int? SortOrder { get; set; }

        public IMappingExpression<T, PurchasingRequestDetail> MappingBase<T>(Profile profile) where T : PurchasingRequestInputDetail
            => profile.CreateMap<PurchasingRequestDetail, T>()
          .ReverseMap()
          .ForMember(m => m.PurchasingRequest, m => m.Ignore())
          .ForMember(m => m.PurchasingSuggestDetail, m => m.Ignore());

        public void Mapping(Profile profile) => MappingBase<PurchasingRequestInputDetail>(profile);

    }


}
