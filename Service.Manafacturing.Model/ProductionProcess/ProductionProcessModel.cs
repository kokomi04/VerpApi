﻿using System.Collections.Generic;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionProcess
{
    public class ProductionProcessModel
    {
        public ProductionProcessModel()
        {
            ProductionStepLinks = new List<ProductionStepLinkModel>();
            //ProductionStepGroupLinkDataRoles = new List<ProductionStepLinkDataRoleInput>();
        }
        public long ContainerId { get; set; }
        public EnumContainerType ContainerTypeId { get; set; }

        public List<ProductionStepModel> ProductionSteps { get; set; }
        public List<ProductionStepLinkDataInput> ProductionStepLinkDatas { get; set; }
        public List<ProductionStepLinkDataRoleInput> ProductionStepLinkDataRoles { get; set; }
        //public List<ProductionStepLinkDataRoleInput> ProductionStepGroupLinkDataRoles { get; set; } // dữ liệu chỉ view, không sử dụng trong CRUD 
        public List<ProductionStepLinkModel> ProductionStepLinks { get; set; } // dữ liệu chỉ view, không sử dụng trong CRUD 
        public List<ProductionOutsourcePartMappingInput> ProductionOutsourcePartMappings { get; set; }
        public long? UpdatedDatetimeUtc { get; set; }

    }
}
