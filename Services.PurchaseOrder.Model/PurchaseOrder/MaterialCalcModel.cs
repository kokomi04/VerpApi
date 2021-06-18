using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class MaterialCalcListModel : IMapFrom<MaterialCalc>
    {
        public long MaterialCalcId { get; set; }
        public string MaterialCalcCode { get; set; }
        public long Title { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<MaterialCalc, MaterialCalcListModel>();
        }
    }
    public class MaterialCalcModel : MaterialCalcListModel
    {
        public IList<MaterialCalcProductModel> Products { get; set; }
        public IList<MaterialCalcSummaryModel> Summary { get; set; }
        public override void Mapping(Profile profile)
        {
            profile.CreateMap<MaterialCalcModel, MaterialCalc>()
                .ForMember(d => d.CreatedByUserId, s => s.Ignore())
                .ForMember(d => d.CreatedDatetimeUtc, s => s.Ignore())
                .ForMember(d => d.MaterialCalcProduct, s => s.MapFrom(m => m.Products))
                .ForMember(d => d.MaterialCalcSummary, s => s.MapFrom(m => m.Summary))
                .ReverseMap();
            //.ForMember(d => d.Products, s => s.MapFrom(m => m.MaterialCalcProduct))
            //.ForMember(d => d.Summary, s => s.MapFrom(m => m.MaterialCalcSummary));
        }
    }

    public class MaterialCalcProductModel : IMapFrom<MaterialCalcProduct>
    {
        public int ProductId { get; set; }
        public IList<MaterialCalcProductOrderModel> Orders { get; set; }
        public IList<MaterialCalcProductDetailModel> Details { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<MaterialCalcProductModel, MaterialCalcProduct>()
                .ForMember(d => d.MaterialCalcProductDetail, s => s.MapFrom(m => m.Details))
                .ForMember(d => d.MaterialCalcProductOrder, s => s.MapFrom(m => m.Orders))
                .ReverseMap()
                .ForMember(d => d.Orders, s => s.MapFrom(m => m.MaterialCalcProductOrder))
                .ForMember(d => d.Details, s => s.MapFrom(m => m.MaterialCalcProductDetail));
        }
    }

    public class MaterialCalcProductOrderModel : IMapFrom<MaterialCalcProductOrder>
    {
        public string OrderCode { get; set; }
        public decimal OrderProductQuantity { get; set; }
    }
    public class MaterialCalcProductDetailModel : IMapFrom<MaterialCalcProductDetail>
    {
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }
    }

    public class MaterialCalcSummaryModel : IMapFrom<MaterialCalcSummary>
    {
        public int OriginalMaterialProductId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }
    }
}
