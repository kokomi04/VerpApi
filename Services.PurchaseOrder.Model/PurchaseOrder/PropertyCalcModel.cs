using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public abstract class PropertyCalcBasicModel
    {
        public long PropertyCalcId { get; set; }
        public string PropertyCalcCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
    }

    public class PropertyCalcListModel : PropertyCalcBasicModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string OrderCodes { get; set; }
        public decimal? TotalOrderProductQuantity { get; set; }

        public bool IsPurchasingRequestCreated { get; set; }
        public long? PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
    }
    public class PropertyCalcModel : PropertyCalcBasicModel, IMapFrom<PropertyCalc>
    {
        public long? PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }

        public IList<PropertyCalcPropertyModel> Properties { get; set; }
        public IList<PropertyCalcProductModel> Products { get; set; }
        public IList<PropertyCalcSummaryModel> Summary { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<PropertyCalcModel, PropertyCalc>()
                .ForMember(d => d.CreatedByUserId, s => s.Ignore())
                .ForMember(d => d.CreatedDatetimeUtc, s => s.Ignore())
                .ForMember(d => d.PropertyCalcProduct, s => s.MapFrom(m => m.Products))
                .ForMember(d => d.PropertyCalcSummary, s => s.MapFrom(m => m.Summary))
                .ForMember(d => d.PropertyCalcProperty, s => s.MapFrom(m => m.Properties))
                .ReverseMap()
                .ForMember(d => d.CreatedDatetimeUtc, s => s.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()));
        }
    }

    public class PropertyCalcPropertyModel : IMapFrom<PropertyCalcProperty>
    {
        public int PropertyId { get; set; }
    }

    public class PropertyCalcProductModel : IMapFrom<PropertyCalcProduct>
    {
        public int ProductId { get; set; }
        public IList<PropertyCalcProductOrderModel> Orders { get; set; }
        public IList<PropertyCalcProductDetailModel> Details { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<PropertyCalcProductModel, PropertyCalcProduct>()
                .ForMember(d => d.PropertyCalcProductDetail, s => s.MapFrom(m => m.Details))
                .ForMember(d => d.PropertyCalcProductOrder, s => s.MapFrom(m => m.Orders))
                .ReverseMap()
                .ForMember(d => d.Orders, s => s.MapFrom(m => m.PropertyCalcProductOrder))
                .ForMember(d => d.Details, s => s.MapFrom(m => m.PropertyCalcProductDetail));
        }
    }

    public class PropertyCalcProductOrderModel : IMapFrom<PropertyCalcProductOrder>
    {
        public string OrderCode { get; set; }
        public decimal OrderProductQuantity { get; set; }     
    }

    public class PropertyCalcProductDetailModel : IMapFrom<PropertyCalcProductDetail>
    {
        public int PropertyId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }
        public bool IsMaterial { get; set; }
    }

    public class PropertyCalcSummaryModel : IMapFrom<PropertyCalcSummary>
    {
        public int OriginalMaterialProductId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }
        public decimal ExchangeRate { get; set; }
    }



    public class PropertyOrderProductHistory
    {
        public long PropertyCalcId { get; set; }
        public string PropertyCalcCode { get; set; }
        public string Title { get; set; }
        public IList<PropertyCalcPropertyModel> Properties { get; set; }

        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public decimal OrderProductQuantity { get; set; }
    }
}
