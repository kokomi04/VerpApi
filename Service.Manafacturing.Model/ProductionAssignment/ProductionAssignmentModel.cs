using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionAssignmentModel : IMapFrom<ProductionAssignmentEntity>
    {
        public long? ProductionStepId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionProcess.ProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public int CompletedQuantity { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionAssignmentEntity, ProductionAssignmentModel>()
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => (EnumProductionProcess.EnumProductionStepLinkDataRoleType)m.ObjectTypeId))
                .ReverseMap()
                .ForMember(m => m.ObjectTypeId, v => v.MapFrom(m => (int)m.ObjectTypeId));
        }
    }
}
