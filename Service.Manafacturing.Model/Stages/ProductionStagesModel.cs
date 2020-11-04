using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Stages
{
    public class ProductionStagesModel: IMapFrom<ProductionStages>
    {
        public int ProductionStagesId { get; set; }
        public int ProductionStagesType { get; set; }
        public string ProductionStagesTitle { get; set; }
        public int? ProductionStagesParent { get; set; }
        public int ProductId { get; set; }
        public int SortOrder { get; set; }

        public List<InOutStagesModel> InOutStages { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStages, ProductionStagesModel>()
                .ForMember(m => m.InOutStages, v => v.Ignore());
        }
    }

}
