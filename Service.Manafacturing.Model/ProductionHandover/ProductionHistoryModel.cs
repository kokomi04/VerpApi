using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHistoryModel : ProductionHistoryInputModel
    {
        public long? ProductionHistoryId { get; set; }
        public int CreatedByUserId { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHistory, ProductionHistoryModel>()
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => (EnumProductionProcess.EnumProductionStepLinkDataObjectType)m.ObjectTypeId))
                .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()));
        }
    }

    public class ProductionHistoryInputModel : IMapFrom<ProductionHistory>
    {
        public decimal ProductionQuantity { get; set; }
        public decimal? OvertimeProductionQuantity { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionProcess.EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public long? Date { get; set; }
        public string Note { get; set; }
        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHistoryInputModel, ProductionHistory>()
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => (int)m.ObjectTypeId))
                .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()));
        }
    }
}
