using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkDataRoleModel : IMapFrom<ProductionStepLinkDataRole>
    {
        public long ProductionStepId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
    }

    public class ProductionStepLinkDataRoleInput: ProductionStepLinkDataRoleModel
    {
        public string ProductionStepCode { get; set; }
        public string ProductionStepLinkDataCode { get; set; }
        public int? ProductionStepLinkTypeId { get; set; }

        //public void Mapping(Profile profile)
        //{
        //    profile.CreateMap<ProductionStepLinkDataRoleModel, ProductionStepLinkDataRoleInput>
        //}
    }
}
