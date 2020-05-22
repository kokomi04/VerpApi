using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{

    public class ReportTypeViewModel : IMapFrom<ReportTypeView>
    {
        public int ReportTypeViewId { get; set; }
        public string ReportTypeViewName { get; set; }
        public bool IsDefault { get; set; }
        public int Columns { get; set; }
        public int ReportTypeGroupId { get; set; }
        public IList<ReportTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ReportTypeView, ReportTypeViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMap()
                .ForMember(d => d.ReportTypeViewField, m => m.Ignore())
                .ForMember(d => d.ReportType, m => m.Ignore());
        }
    }

    public class ReportTypeViewFieldModel : IMapFrom<ReportTypeViewField>
    {
        public int ReportTypeViewFieldId { get; set; }
        public string ParamerterName { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public int? ReferenceCategoryId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<ReportTypeViewField, ReportTypeViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ReverseMap()
            .ForMember(m => m.ReportTypeView, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId));
    }
}
