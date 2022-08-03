using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.HrConfig
{
    public class HrTypeViewModelList : IMapFrom<HrTypeView>
    {
        public int HrTypeViewId { get; set; }
        public string HrTypeViewName { get; set; }
        public bool IsDefault { get; set; }
        public int Columns { get; set; }
        public int HrTypeGroupId { get; set; }
    }

    public class HrTypeViewModel : HrTypeViewModelList
    {
        public IList<HrTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapIgnoreNoneExist<HrTypeView, HrTypeViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMapIgnoreNoneExist()
                .ForMember(d => d.HrTypeViewField, m => m.Ignore())
                .ForMember(d => d.HrType, m => m.Ignore());
        }
    }

    public class HrTypeViewFieldModel : IMapFrom<HrTypeViewField>
    {
        public int HrTypeViewFieldId { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string SelectFilters { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }

        public void Mapping(Profile profile) => profile.CreateMapIgnoreNoneExist<HrTypeViewField, HrTypeViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ReverseMapIgnoreNoneExist()
            .ForMember(m => m.HrTypeView, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId));
    }
}
