﻿using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportTypeViewModel : IMapFrom<ReportTypeView>
    {
        public int ReportTypeViewId { get; set; }
        public string ReportTypeViewName { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public IList<ReportTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ReportTypeView, ReportTypeViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMapCustom()
                .ForMember(d => d.ReportTypeViewField, m => m.Ignore())
                .ForMember(d => d.ReportType, m => m.Ignore())
                .ForMember(d => d.ReportTypeViewId, m => m.Ignore());
        }
    }

    public class ReportTypeViewFieldModel : IMapFrom<ReportTypeViewField>
    {
        public int ReportTypeViewFieldId { get; set; }
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

        public void Mapping(Profile profile) => profile.CreateMapCustom<ReportTypeViewField, ReportTypeViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ReverseMapCustom()
            .ForMember(m => m.ReportTypeView, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId));
    }
}
