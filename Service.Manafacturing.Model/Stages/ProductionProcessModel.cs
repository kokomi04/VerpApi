using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Stages
{
    public class ProductionProcessModel
    {
        public List<ProductionStagesModel> ProductionStages { get; set; }
        public List<ProductionStagesMappingModel> StagesMapping { get; set; }
    }
}
