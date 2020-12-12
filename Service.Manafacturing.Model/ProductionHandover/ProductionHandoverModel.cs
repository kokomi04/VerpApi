using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHandoverModel : ProductionHandoverInputModel
    {
        public long? ProductionHandoverId { get; set; }
        public EnumHandoverStatus Status { get; set; }
        public int CreatedByUserId { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHandoverEntity, ProductionHandoverModel>()
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => (EnumProductionProcess.ProductionStepLinkDataObjectType)m.ObjectTypeId))
                .ForMember(m => m.Status, v => v.MapFrom(m => (EnumHandoverStatus)m.Status))
                .ForMember(m => m.HandoverDatetime, v => v.MapFrom(m => m.HandoverDatetime.GetUnix()));
        }
    }

    public class ProductionHandoverInputModel : IMapFrom<ProductionHandoverEntity>
    {
        public decimal HandoverQuantity { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionProcess.ProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public int FromDepartmentId { get; set; }
        public long FromProductionStepId { get; set; }
        public int ToDepartmentId { get; set; }
        public long ToProductionStepId { get; set; }
        public long? HandoverDatetime { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHandoverInputModel, ProductionHandoverEntity>()
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => (int)m.ObjectTypeId))
                .ForMember(m => m.HandoverDatetime, v => v.MapFrom(m => m.HandoverDatetime.UnixToDateTime()));
        }
    }
}
