using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{   
    public class OutsourcePropertyOrderList
    {
        public long OutsourceOrderId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public long OutsourceOrderFinishDate { get; set; }
        public long OutsourceOrderDate { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long? PropertyCalcId { get; set; }
        public string PropertyCalcCode { get; set; }

        public long ObjectId { get; set; }
        public decimal Quantity { get; set; }

        public int? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? unitId { get; set; }
        
        public int? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }

    public class OutsourcePropertyOrderInput : OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public long PropertyCalcId { get; set; }
        public IList<OutsourcePropertyOrderDetail> OutsourceOrderDetail { get; set; }
        public IList<OutsourceOrderMaterialsModel> OutsourceOrderMaterials { get; set; }
        public IList<OutsourceOrderExcessModel> OutsourceOrderExcesses { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourcePropertyOrderInput>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonDeserialize<SuppliersModel>()))
                .ForMember(m => m.AttachmentFileId, v => v.MapFrom(m => m.AttachmentFileId))
                .ForMember(m => m.ExcessMaterialNotes, v => v.MapFrom(m => m.ExcessMaterialNotes))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonSerialize()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonSerialize()))
                .ForMember(m => m.AttachmentFileId, v => v.MapFrom(m => m.AttachmentFileId))
                .ForMember(m => m.ExcessMaterialNotes, v => v.MapFrom(m => m.ExcessMaterialNotes));
        }
    }

    public class OutsourcePropertyOrderDetail : IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
      
        public decimal OutsourceOrderQuantity { get; set; }
        public decimal OutsourceOrderPrice { get; set; }
        public decimal OutsourceOrderTax { get; set; }

        [Required]
        public long ProductId { get; set; }

        public int? OutsourceOrderProductUnitConversionId { get; set; }
        public decimal? OutsourceOrderProductUnitConversionQuantity { get; set; }
        public decimal? OutsourceOrderProductUnitConversionPrice { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrderDetail, OutsourcePropertyOrderDetail>()
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ObjectId))
                .ForMember(m => m.OutsourceOrderPrice, v => v.MapFrom(m => m.Price))
                .ForMember(m => m.OutsourceOrderQuantity, v => v.MapFrom(m => m.Quantity))
                .ForMember(m => m.OutsourceOrderTax, v => v.MapFrom(m => m.Tax))
                .ForMember(m => m.OutsourceOrderProductUnitConversionPrice, v => v.MapFrom(m => m.ProductUnitConversionPrice))
                .ForMember(m => m.OutsourceOrderProductUnitConversionQuantity, v => v.MapFrom(m => m.ProductUnitConversionQuantity))
                .ForMember(m => m.OutsourceOrderProductUnitConversionId, v => v.MapFrom(m => m.ProductUnitConversionId))
                .ReverseMap()
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ProductId))
                .ForMember(m => m.Price, v => v.MapFrom(m => m.OutsourceOrderPrice))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.OutsourceOrderQuantity))
                .ForMember(m => m.Tax, v => v.MapFrom(m => m.OutsourceOrderTax))
                .ForMember(m => m.ProductUnitConversionPrice, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionPrice))
                .ForMember(m => m.ProductUnitConversionQuantity, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionQuantity))
                .ForMember(m => m.ProductUnitConversionId, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionId));

        }
    }
}
