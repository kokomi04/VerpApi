using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Stages
{
    public class InOutStagesModel : IMapFrom<ProductionStagesDetail>
    {
        public int ProductionStagesDetailId { get; set; }
        public int ProductionStagesId { get; set; }
        public EnumInOutStages InOutStagesType { get; set; }
        public ProductTypeInStages ProductType { get; set; }
        public int ProductId { get; set; }
        public decimal ActualNumber { get; set; }
        public int UnitId { get; set; }
        public int SortOrder { get; set; }
        public int? AssignedTo { get; set; }
    }
}
