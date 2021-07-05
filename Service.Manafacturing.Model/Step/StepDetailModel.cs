using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Step
{
    public class StepDetailModel: IMapFrom<StepDetail>
    {
        public int StepDetailId { get; set; }
        public int StepId { get; set; }
        public int DepartmentId { get; set; }
        public decimal Quantity { get; set; }
        public decimal WorkingHours { get; set; }
        public int NumberOfPerson { get; set; }
    }
}
