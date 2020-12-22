using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Enums.MasterEnum;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
namespace VErp.Services.Manafacturing.Model.Report
{
    public class StepProgressModel 
    {
        public int StepId { get; set; }

        public IList<StepScheduleProgressModel> StepScheduleProgress { get; set; }
        public StepProgressModel()
        {
            StepScheduleProgress = new List<StepScheduleProgressModel>();
        }

    }

    public class StepScheduleProgressModel
    {
        public string ProductionOrderCode { get; set; }
        public int[] ProductIds { get; set; }

        public decimal ProgressPercent { get; set; }
        public IList<StepScheduleProgressDataModel> InputData { get; set; }
        public IList<StepScheduleProgressDataModel> OutputData { get; set; }
        public StepScheduleProgressModel()
        {
            InputData = new List<StepScheduleProgressDataModel>();
            OutputData = new List<StepScheduleProgressDataModel>();
        }

    }

    public class StepScheduleProgressDataModel
    {
        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
    }
}
