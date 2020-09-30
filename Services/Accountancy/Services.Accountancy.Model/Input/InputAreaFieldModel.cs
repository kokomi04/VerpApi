using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputFieldInputModel : IMapFrom<InputField>
    {
        [Required(ErrorMessage = "Vui lòng nhập tên trường dữ liệu")]
        [MaxLength(45, ErrorMessage = "Tên trường dữ liệu quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Tên trường dữ liệu gồm các ký tự chữ, số và ký tự _.")]
        public string FieldName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }       
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int DecimalPlace { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }

        protected void MappingBase<T>(Profile profile) where T: InputFieldInputModel
        {
            profile.CreateMap<InputField, T>()
                .ForMember(d => d.DataTypeId, m => m.MapFrom(f => (EnumDataType)f.DataTypeId))
                .ForMember(d => d.FormTypeId, m => m.MapFrom(f => (EnumFormType)f.FormTypeId))
                .ReverseMap()
                .ForMember(d => d.DataTypeId, m => m.MapFrom(f => (int)f.DataTypeId))
                .ForMember(d => d.FormTypeId, m => m.MapFrom(f => (int)f.FormTypeId));
        }

        public void Mapping(Profile profile)
        {
            MappingBase<InputFieldInputModel>(profile);
        }

    }

    public class InputFieldOutputModel : InputFieldInputModel
    {
        public int InputFieldId { get; set; }   
        public new void Mapping(Profile profile)
        {
            MappingBase<InputFieldOutputModel>(profile);
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
        public bool IsCalcSum { get; set; }
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
        public bool? AutoFocus { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string DefaultValue { get; set; }
        public int? IdGencode { get; set; }
        public string RequireFilters { get; set; }

        public bool Compare(InputAreaField curField)
        {
            return !curField.IsDeleted &&
                InputAreaId == curField.InputAreaId &&
                InputFieldId == curField.InputFieldId &&
                InputTypeId == curField.InputTypeId &&
                Title == curField.Title &&
                Placeholder == curField.Placeholder &&
                SortOrder == curField.SortOrder &&
                IsAutoIncrement == curField.IsAutoIncrement &&
                IsRequire == curField.IsRequire &&
                IsUnique == curField.IsUnique &&
                IsHidden == curField.IsHidden &&
                IsCalcSum == curField.IsCalcSum &&
                RegularExpression == curField.RegularExpression &&
                DefaultValue == curField.DefaultValue &&
                Filters == curField.Filters &&
                Width == curField.Width &&
                Height == curField.Height &&
                TitleStyleJson == curField.TitleStyleJson &&
                InputStyleJson == curField.InputStyleJson &&
                OnFocus == curField.OnFocus &&
                OnKeydown == curField.OnKeydown &&
                OnKeypress == curField.OnKeypress &&
                OnBlur == curField.OnBlur &&
                OnChange == curField.OnChange &&
                AutoFocus == curField.AutoFocus &&
                Column == curField.Column &&
                RequireFilters == curField.RequireFilters;
        }
    }

    public class InputAreaFieldOutputFullModel : InputAreaFieldInputModel
    {
        public InputFieldOutputModel InputField { get; set; }
    }
}
