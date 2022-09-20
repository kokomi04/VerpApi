using AutoMapper;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{

    public class ProductionStepLinkDataObjectModel
    {
        public long LinkDataObjectId { get; set; }
        public EnumProductionStepLinkDataObjectType LinkDataObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ProductionStepLinkDataModel : IMapFrom<ProductionStepLinkData>
    {
        public long ProductionStepLinkDataId { get; set; }
        public string ProductionStepLinkDataCode { get; set; }
        public long LinkDataObjectId { get; set; }
        public EnumProductionStepLinkDataObjectType LinkDataObjectTypeId { get; set; }
        // public long ObjectId { get; set; }
        // public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? WorkloadConvertRate { get; set; }

        public decimal QuantityOrigin { get; set; }
        public decimal OutsourceQuantity { get; set; }
        public decimal ExportOutsourceQuantity { get; set; }
        public decimal? OutsourcePartQuantity { get; set; }
        public int SortOrder { get; set; }
        public long? OutsourceRequestDetailId { get; set; }
        public EnumProductionStepLinkDataType ProductionStepLinkDataTypeId { get; set; }
        public EnumProductionStepLinkType ProductionStepLinkTypeId { get; set; }
        public long? ConverterId { get; set; }
        public long? ProductionOutsourcePartMappingId { get; set; }
    }

    public class ProductionStepLinkDataInput : ProductionStepLinkDataModel
    {
        public string ObjectTitle { get; set; }
        public int UnitId { get; set; }
        public int ProductUnitConversion { get; set; }
        public int DecimalPlace { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }

    public class ProductionStepLinkDataInfo : ProductionStepLinkDataModel, IMapFrom<ProductionStepLinkDataRole>
    {
        public EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
        //public string ProductionStepLinkDataGroup { get; set; }
        public long ProductionStepId { get; set; }
        public string ObjectTitle { get; set; }
        public int UnitId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionStepLinkDataRole, ProductionStepLinkDataInfo>()
                .ForMember(m => m.LinkDataObjectId, v => v.MapFrom(m => m.ProductionStepLinkData.LinkDataObjectId))
                .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataId))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.ProductionStepLinkData.Quantity))
                .ForMember(m => m.QuantityOrigin, v => v.MapFrom(m => m.ProductionStepLinkData.QuantityOrigin))
                .ForMember(m => m.WorkloadConvertRate, v => v.MapFrom(m => m.ProductionStepLinkData.WorkloadConvertRate))
                .ForMember(m => m.SortOrder, v => v.MapFrom(m => m.ProductionStepLinkData.SortOrder))
                .ForMember(m => m.LinkDataObjectTypeId, v => v.MapFrom(m => (EnumProductionStepLinkDataObjectType)m.ProductionStepLinkData.LinkDataObjectTypeId))
                .ForMember(m => m.OutsourceQuantity, v => v.MapFrom(m => m.ProductionStepLinkData.OutsourceQuantity))
                .ForMember(m => m.ExportOutsourceQuantity, v => v.MapFrom(m => m.ProductionStepLinkData.ExportOutsourceQuantity))
                .ForMember(m => m.OutsourcePartQuantity, v => v.MapFrom(m => m.ProductionStepLinkData.OutsourcePartQuantity))
                .ForMember(m => m.ProductionStepLinkDataCode, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataCode))
                .ForMember(m => m.OutsourceRequestDetailId, v => v.MapFrom(m => m.ProductionStepLinkData.OutsourceRequestDetailId))
                .ForMember(m => m.ProductionStepLinkDataTypeId, v => v.MapFrom(m => (EnumProductionStepLinkDataType)m.ProductionStepLinkData.ProductionStepLinkDataTypeId))
                .ForMember(m => m.ProductionStepLinkTypeId, v => v.MapFrom(m => (EnumProductionStepLinkType)m.ProductionStepLinkData.ProductionStepLinkTypeId))
                .ForMember(m => m.ConverterId, v => v.MapFrom(m => m.ProductionStepLinkData.ConverterId))
                .ForMember(m => m.ProductionStepLinkDataRoleTypeId, v => v.MapFrom(m => (EnumProductionProcess.EnumProductionStepLinkDataRoleType)m.ProductionStepLinkDataRoleTypeId))
                //.ForMember(m => m.ProductionStepLinkDataGroup, v => v.MapFrom(m => m.ProductionStepLinkDataGroup))
                .ReverseMapCustom()
                .ForMember(m => m.ProductionStepLinkData, v => v.Ignore())
                .ForMember(m => m.ProductionStepLinkDataRoleTypeId, v => v.MapFrom(m => (int)m.ProductionStepLinkDataRoleTypeId));
            //.ForMember(m => m.ProductionStepLinkDataGroup, v => v.MapFrom(m => m.ProductionStepLinkDataGroup));
        }
    }

}
