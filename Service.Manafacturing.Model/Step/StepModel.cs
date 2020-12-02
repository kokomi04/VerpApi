using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using StepEnity = VErp.Infrastructure.EF.ManufacturingDB.Step;

namespace VErp.Services.Manafacturing.Model.Step
{
    public class StepModel: IMapFrom<StepEnity>
    {
        public int StepId { get; set; }
        public string StepName { get; set; }
        public int SortOrder { get; set; }
        public int StepGroupId { get; set; }
    }
}
