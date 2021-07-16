using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Category
{
    public class CategoryViewModel : IMapFrom<CategoryView>
    {
        public int CategoryViewId { get; set; }
        public string CategoryViewName { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public IList<CategoryViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryView, CategoryViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMap()
                .ForMember(d => d.CategoryViewField, m => m.Ignore())
                .ForMember(d => d.Category, m => m.Ignore())
                .ForMember(d => d.CategoryViewId, m => m.Ignore());
        }
    }

    public class CategoryViewFieldModel : IMapFrom<CategoryViewField>
    {
        public int CategoryViewFieldId { get; set; }
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

        public void Mapping(Profile profile) => profile.CreateMap<CategoryViewField, CategoryViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ReverseMap()
            .ForMember(m => m.CategoryView, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId));
    }
}
