using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using StepEnity = VErp.Infrastructure.EF.ManufacturingDB.Step;

namespace VErp.Services.Manafacturing.Model.Step
{
    public class StepModel: IMapFrom<StepEnity>
    {
        public int StepId { get; set; }
        public string StepName { get; set; }
        public int SortOrder { get; set; }
        public int StepGroupId { get; set; }
        public bool IsHide { get; set; }
        public int UnitId { get; set; }
        public decimal ShrinkageRate { get; set; }
        public string Description { get; set; }
        public EnumHandoverTypeStatus HandoverTypeId { get; set; }

        public List<StepDetailModel> StepDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StepEnity, StepModel>()
                .ForMember(m => m.StepDetail, v => v.MapFrom(m => m.StepDetail))
                .ReverseMap()
                .ForMember(m => m.StepDetail, v => v.Ignore());
        }
    }
}
