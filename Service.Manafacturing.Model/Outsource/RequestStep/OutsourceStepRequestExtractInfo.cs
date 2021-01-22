using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestExtractInfo: OutsourceStepRequestExtractBase
    {
        public DateTime OutsourceStepRequestFinishDate { get; set; }
        public DateTime OutsourceStepRequestDate { get; set; }
    }

    public class OutsourceStepRequestSearch : OutsourceStepRequestExtractBase, IMapFrom<OutsourceStepRequestExtractInfo>
    {
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }
        public IEnumerable<ProductionStepSimpleModel> ProductionSteps { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequestExtractInfo, OutsourceStepRequestSearch>()
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.OutsourceStepRequestDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()));

        }
    }

    public class OutsourceStepRequestExtractBase
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionProcessTitle { get; set; }
        public long ProductionProcessId { get; set; }
        public EnumOutsourceRequestStatusType OutsourceStepRequestStatusId { get; set; }
        public bool MarkInvalid { get; set; }

    }
}
