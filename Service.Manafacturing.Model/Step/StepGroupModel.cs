using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Step
{
    public class StepGroupModel:IMapFrom<StepGroup>
    {
        public int StepGroupId { get; set; }
        public string StepGroupName { get; set; }
        public int SortOrder { get; set; }
    }
}
