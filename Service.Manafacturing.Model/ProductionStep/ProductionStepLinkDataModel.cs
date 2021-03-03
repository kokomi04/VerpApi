using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkDataModel: IMapFrom<ProductionStepLinkData>
    {
        public long ProductionStepLinkDataId { get; set; }
        public string ProductionStepLinkDataCode { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityOrigin { get; set; }
        public decimal OutsourceQuantity { get; set; }
        public decimal ExportOutsourceQuantity { get; set; }
        public int SortOrder { get; set; }
        public long? OutsourceRequestDetailId { get; set; }
        public EnumProductionStepLinkDataType ProductionStepLinkDataTypeId { get; set; }
        public EnumProductionStepLinkType ProductionStepLinkTypeId { get; set; }
        public long? ConverterId { get; set; }
    }

    public class ProductionStepLinkDataInput : ProductionStepLinkDataModel
    {
        public string ObjectTitle { get; set; }
        public int UnitId { get; set; }
    }

    public class ProductionStepLinkDataInfo : ProductionStepLinkDataModel, IMapFrom<ProductionStepLinkDataRole>
    {
        public EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
        public long ProductionStepId { get; set; }
        public string ObjectTitle { get; set; }
        public int UnitId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepLinkDataRole, ProductionStepLinkDataInfo >()
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ProductionStepLinkData.ObjectId))
                .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataId))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.ProductionStepLinkData.Quantity))
                .ForMember(m => m.QuantityOrigin, v => v.MapFrom(m => m.ProductionStepLinkData.QuantityOrigin))
                .ForMember(m => m.SortOrder, v => v.MapFrom(m => m.ProductionStepLinkData.SortOrder))
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => m.ProductionStepLinkData.ObjectTypeId))
                .ForMember(m => m.OutsourceQuantity, v => v.MapFrom(m => m.ProductionStepLinkData.OutsourceQuantity))
                .ForMember(m => m.ExportOutsourceQuantity, v => v.MapFrom(m => m.ProductionStepLinkData.ExportOutsourceQuantity))
                .ForMember(m => m.ProductionStepLinkDataCode, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataCode))
                .ForMember(m => m.OutsourceRequestDetailId, v => v.MapFrom(m => m.ProductionStepLinkData.OutsourceRequestDetailId))
                .ForMember(m => m.ProductionStepLinkDataTypeId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataTypeId))
                .ForMember(m => m.ProductionStepLinkTypeId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkTypeId))
                .ForMember(m => m.ConverterId, v => v.MapFrom(m => m.ProductionStepLinkData.ConverterId))
                .ForMember(m => m.ProductionStepLinkDataRoleTypeId, v => v.MapFrom(m => (EnumProductionProcess.EnumProductionStepLinkDataRoleType)m.ProductionStepLinkDataRoleTypeId))
                .ReverseMap()
                .ForMember(m => m.ProductionStepLinkData, v => v.Ignore())
                .ForMember(m => m.ProductionStepLinkDataRoleTypeId, v => v.MapFrom(m => (int)m.ProductionStepLinkDataRoleTypeId));
        }
    }

}
