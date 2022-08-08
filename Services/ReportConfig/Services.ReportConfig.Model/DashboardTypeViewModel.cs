using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class DashboardTypeViewModel : IMapFrom<DashboardTypeView>
    {
        public int DashboardTypeViewId { get; set; }
        public string DashboardTypeViewName { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public IList<DashboardTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<DashboardTypeView, DashboardTypeViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMapCustom()
                .ForMember(d => d.DashboardTypeViewField, m => m.Ignore())
                .ForMember(d => d.DashboardType, m => m.Ignore())
                .ForMember(d => d.DashboardTypeViewId, m => m.Ignore());
        }
    }

    public class DashboardTypeViewFieldModel : IMapFrom<DashboardTypeViewField>
    {
        public int DashboardTypeViewFieldId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Vui lòng nhập tham số báo cáo")]
        public string ParamerterName { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public string RefFilters { get; set; }
        public string ExtraFilter { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }
        public string TitleStyleJson { get; set; }
        public string InputStyleJson { get; set; }
        public string HelpText { get; set; }

        public void Mapping(Profile profile) => profile.CreateMapCustom<DashboardTypeViewField, DashboardTypeViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ReverseMapCustom()
            .ForMember(m => m.DashboardTypeView, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId));
    }
}
