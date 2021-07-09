using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceStepOrderOutput: OutsourceOrderModel, IMapFrom<OutsourceOrder>
    {
        public IList<OutsourceStepOrderDetailOutput> OutsourceOrderDetail { get; set; }
        public IList<OutsourceOrderMaterialsOutput> OutsourceOrderMaterials { get; set; }
        public IList<OutsourceOrderExcessOutput> OutsourceOrderExcesses { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrder, OutsourceStepOrderOutput>()
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonDeserialize<SuppliersModel>()))
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderMaterials, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderExcesses, v => v.Ignore())
                .ForMember(m => m.AttachmentFileId, v => v.MapFrom(m=>m.AttachmentFileId))
                .ReverseMap()
                .ForMember(m => m.OutsourceOrderDetail, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderMaterials, v => v.Ignore())
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.Value.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()))
                .ForMember(m => m.DeliveryDestination, v => v.MapFrom(m => m.DeliveryDestination.JsonSerialize()))
                .ForMember(m => m.Suppliers, v => v.MapFrom(m => m.Suppliers.JsonSerialize()))
                .ForMember(m => m.AttachmentFileId, v => v.MapFrom(m => m.AttachmentFileId));
        }
    }

    public class OutsourceOrderMaterialsOutput: OutsourceOrderMaterialsModel
    {
        public string ProductTitle { get; set; }
        public int UnitId { get; set; }
        public decimal? QuantityRequirement { get; set; }
        public string OutsourceRequestCode { get; set; }
        public int? DecimalPlace { get; set; }
    }

    public class OutsourceOrderExcessOutput: OutsourceOrderExcessModel
    {
        public string ProductTitle { get; set; }
        public int UnitId { get; set; }
        public int? DecimalPlace { get; set; }
    }

    public class OutsourceStepOrderDetailOutput : IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsourceOrderId { get; set; }
        public decimal OutsourceOrderQuantity { get; set; }
        public decimal OutsourceOrderPrice { get; set; }
        public decimal OutsourceOrderTax { get; set; }
        public long ProductionStepLinkDataId { get; set; }

        public string ProductionStepTitle { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public decimal? OutsourceStepRequestDataQuantity { get; set; }
        public long OutsourceStepRequestId { get; set; }
        public string ProductionStepLinkDataTitle { get; set; }
        public int ProductionStepLinkDataUnitId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public bool? IsImportant { get; set; }
        public int DecimalPlace { get; set; }

        public int? OutsourceOrderProductUnitConversionId { get; set; }
        public decimal? OutsourceOrderProductUnitConversionQuantity { get; set; }
        public decimal? OutsourceOrderProductUnitConversionPrice { get; set; }
        public int ProductionStepLinkDataObjectId { get; set; }

        //public decimal? DecimalPlaceUnitConversion { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrderDetail, OutsourceStepOrderDetailOutput>()
               .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ObjectId))
               .ForMember(m => m.OutsourceOrderPrice, v => v.MapFrom(m => m.Price))
               .ForMember(m => m.OutsourceOrderQuantity, v => v.MapFrom(m => m.Quantity))
               .ForMember(m => m.OutsourceOrderTax, v => v.MapFrom(m => m.Tax))
               .ForMember(m => m.OutsourceOrderProductUnitConversionPrice, v => v.MapFrom(m => m.ProductUnitConversionPrice))
               .ForMember(m => m.OutsourceOrderProductUnitConversionQuantity, v => v.MapFrom(m => m.ProductUnitConversionQuantity))
               .ForMember(m => m.OutsourceOrderProductUnitConversionId, v => v.MapFrom(m => m.ProductUnitConversionId))
               .ReverseMap()
               .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ProductionStepLinkDataId))
               .ForMember(m => m.Price, v => v.MapFrom(m => m.OutsourceOrderPrice))
               .ForMember(m => m.Quantity, v => v.MapFrom(m => m.OutsourceOrderQuantity))
               .ForMember(m => m.Tax, v => v.MapFrom(m => m.OutsourceOrderTax))
               .ForMember(m => m.ProductUnitConversionPrice, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionPrice))
               .ForMember(m => m.ProductUnitConversionQuantity, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionQuantity))
               .ForMember(m => m.ProductUnitConversionId, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionId));

        }
    }
}
