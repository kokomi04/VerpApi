using AutoMapper;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputFieldInputModel : IMapFrom<InputField>
    {
        [Required(ErrorMessage = "Vui lòng nhập tên trường dữ liệu")]
        [MaxLength(45, ErrorMessage = "Tên trường dữ liệu quá dài")]
        public string FieldName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        public int FieldIndex { get; set; }
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }

    }

    public class InputFieldOutputModel : InputFieldInputModel
    {
        public int InputFieldId { get; set; }
        public int? ReferenceCategoryId { get; set; }
        public string ReferenceCategoryTitleFieldName { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputField, InputFieldOutputModel>()
                .ForMember(dest => dest.ReferenceCategoryId, opt => opt.MapFrom(src => src.ReferenceCategoryField.CategoryId))
                .ForMember(dest => dest.ReferenceCategoryTitleFieldName, opt => opt.MapFrom(src => src.ReferenceCategoryTitleField.CategoryFieldName));
        }
    }

    public class InputAreaFieldInputModel : IMapFrom<InputAreaField>
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int InputFieldId { get; set; }
        public int? InputAreaFieldId { get; set; }
        public int InputAreaId { get; set; }
        public int InputTypeId { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsRequire { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string TitleStyleJson { get; set; }
        public string InputStyleJson { get; set; }
        public string OnFocus { get; set; }
        public string OnKeydown { get; set; }
        public string OnKeypress { get; set; }
        public string OnBlur { get; set; }
        public string OnChange { get; set; }
        public bool AutoFocus { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string DefaultValue { get; set; }
    }

    public class InputAreaFieldOutputFullModel : InputAreaFieldInputModel
    {
        public InputFieldOutputModel InputField { get; set; }
    }
}
