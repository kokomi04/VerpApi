﻿using System.Collections.Generic;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionProcessOutsourceStep
    {
        public List<ProductionStepModel> ProductionSteps { get; set; }
        public IList<ProductionStepLinkDataOutsourceStep> ProductionStepLinkDatas { get; set; }
        public List<ProductionStepLinkDataRoleInput> ProductionStepLinkDataRoles { get; set; }
        public List<ProductionStepLinkModel> ProductionStepLinks { get; set; } // dữ liệu chỉ view, không sử dụng trong CRUD 

        public long[] ProductionStepLinkDataOutput { get; set; }
        public long[] ProductionStepLinkDataIntput { get; set; }

        public IList<GroupProductionStepToOutsource> groupProductionStepToOutsources { get; set; }
    }

    public class ProductionStepLinkDataOutsourceStep : ProductionStepLinkDataInput
    {
        public string ProductionStepReceiveTitle { get; set; }
        public long ProductionStepReceiveId { get; set; }
        public string ProductionStepSourceTitle { get; set; }
        public long ProductionStepSourceId { get; set; }
        public bool IsImportant { get; set; }

    }
}
