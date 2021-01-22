using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestOutput: OutsourceStepRequestSearch, IMapFrom<OutsourceStepRequestExtractInfo>
    {
        public OutsourceStepRequestOutput()
        {
            OutsourceStepRequestDatas = new List<OutsourceStepRequestDataOutput>();
        }

        public List<OutsourceStepRequestDataOutput> OutsourceStepRequestDatas { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequestExtractInfo, OutsourceStepRequestOutput>()
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.OutsourceStepRequestDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()));

        }
    }

    public class OutsourceStepRequestDataOutput: OutsourceStepRequestDataModel
    {
        public decimal OutsourceStepRequestDataQuantityProcessed { get; set; }
        public string ProductionStepTitle { get; set; }
        public string ProductionStepLinkDataTitle { get; set; }
        public int ProductionStepLinkDataUnitId { get; set; }
    }
}
