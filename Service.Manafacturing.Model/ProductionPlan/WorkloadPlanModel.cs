using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.WorkloadPlanModel
{
    public class WorkloadPlanModel
    {
        public IDictionary<long, List<WorkloadOutputModel>> WorkloadOutput { get; set; }

        public WorkloadPlanModel()
        {
            WorkloadOutput = new Dictionary<long, List<WorkloadOutputModel>>();
        }
    }

    public class WorkloadOutputModel
    {
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public WorkloadOutputModel()
        {
        }
    }

}
