using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using StepEntity = VErp.Infrastructure.EF.ManufacturingDB.Step;

namespace VErp.Services.Manafacturing.Model.Step
{
    public class StepModel: IMapFrom<StepEntity>
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
            profile.CreateMap<StepEntity, StepModel>()
                .ForMember(m => m.StepDetail, v => v.MapFrom(m => m.StepDetail))
                .ReverseMap()
                .ForMember(m => m.StepDetail, v => v.Ignore());
        }
    }
}
