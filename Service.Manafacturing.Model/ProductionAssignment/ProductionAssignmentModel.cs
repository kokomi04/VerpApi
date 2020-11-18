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
        public int AssignmentQuantity { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public int CompletedQuantity { get; set; }
    }
}
