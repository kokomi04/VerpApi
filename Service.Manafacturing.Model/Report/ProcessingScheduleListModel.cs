using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Enums.MasterEnum;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class ProcessingScheduleListModel
    {
        public long ProductionScheduleId { get; set; }
        public long ScheduleTurnId { get; set; }

        public string ProductTitle { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }

        public IList<StepModel> Steps { get; set; }

        public ProcessingScheduleListModel()
        {
            Steps = new List<StepModel>();
        }
    }
}
