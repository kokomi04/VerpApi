using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionStepEnity = VErp.Infrastructure.EF.ManufacturingDB.ProductionStep;


namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepModel: IMapFrom<ProductionStepEnity>
    {
        public long ProductionStepId { get; set; }
        public int StepId { get; set; }
        public string Title { get; set; }
        public int? ParentId { get; set; }
        public ContainerIdType ContainerIdTypeId { get; set; }
        public long ContainerId { get; set; }
        public int SortOrder { get; set; }
        public bool? IsGroup { get; set; }
    }

    public class ProductionStepInfo: ProductionStepModel
    {
        public List<ProductionStepLinkDataInfo > ProductInSteps { get; set; }
    }
}
