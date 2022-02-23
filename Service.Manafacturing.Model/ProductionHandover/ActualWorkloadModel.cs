using AutoMapper;
using System;
using System.Collections.Generic;
namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
   
    public class ActualWorkloadModel
    {
        public IDictionary<int, List<ActualWorkloadOutputModel>> ActualWorkloadOutput { get; set; }

        public ActualWorkloadModel()
        {
            ActualWorkloadOutput = new Dictionary<int, List<ActualWorkloadOutputModel>>();
        }
    }

    public class ActualWorkloadOutputModel
    {
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public ActualWorkloadOutputModel()
        {
        }
    }

}
