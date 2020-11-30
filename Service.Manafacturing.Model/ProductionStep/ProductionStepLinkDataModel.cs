using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkDataModel: IMapFrom<ProductionStepLinkData>
    {
        public long ProductionStepLinkDataId { get; set; }
        public string ProductionStepLinkDataCode { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionProcess.ProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public int SortOrder { get; set; }
        public int ProductId { get; set; }
    }

    public class ProductionStepLinkDataInput : ProductionStepLinkDataModel
    {
        public string ObjectTitle { get; set; }
        public string UnitName { get; set; }
    }

    public class ProductionStepLinkDataInfo : ProductionStepLinkDataModel, IMapFrom<ProductionStepLinkDataRole>
    {
        public EnumProductionProcess.EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
        public long ProductionStepId { get; set; }
        public string ObjectTitle { get; set; }
        public string UnitName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepLinkDataRole, ProductionStepLinkDataInfo >()
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ProductionStepLinkData.ObjectId))
                .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataId))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.ProductionStepLinkData.Quantity))
                .ForMember(m => m.UnitId, v => v.MapFrom(m => m.ProductionStepLinkData.UnitId))
                .ForMember(m => m.SortOrder, v => v.MapFrom(m => m.ProductionStepLinkData.SortOrder))
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => m.ProductionStepLinkData.ObjectTypeId))
                .ForMember(m => m.ProductionStepLinkDataRoleTypeId, v => v.MapFrom(m => (EnumProductionProcess.EnumProductionStepLinkDataRoleType)m.ProductionStepLinkDataRoleTypeId))
                .ReverseMap()
                .ForMember(m => m.ProductionStepLinkData, v => v.Ignore())
                .ForMember(m => m.ProductionStepLinkDataRoleTypeId, v => v.MapFrom(m => (int)m.ProductionStepLinkDataRoleTypeId));
        }
    }

}
