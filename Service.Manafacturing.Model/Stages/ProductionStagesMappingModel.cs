using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Stages
{
    public class ProductionStagesMappingModel: IMapFrom<ProductionStagesMapping>
    {
        public int ProductId { get; set; }
        public int Head { get; set; }
        public int Next { get; set; }
    }
}
