using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionProcessOutsourceStep
    {
        public List<ProductionStepModel> ProductionSteps { get; set; }
        public List<ProductionStepLinkDataOutsourceStep> ProductionStepLinkDatas { get; set; }
        public List<ProductionStepLinkDataRoleInput> ProductionStepLinkDataRoles { get; set; }
        public List<ProductionStepLinkModel> ProductionStepLinks { get; set; } // dữ liệu chỉ view, không sử dụng trong CRUD 

        public long[] ProductionStepLinkDataOutput { get; set; }
    }

    public class ProductionStepLinkDataOutsourceStep: ProductionStepLinkDataInput
    {
        public string ProductionStepReceiveTitle { get; set; }
        public string ProductionStepSourceTitle { get; set; }
    }
}
