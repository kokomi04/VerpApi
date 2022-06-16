﻿using AutoMapper;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputFieldInputModel : IFieldData, IMapFrom<InputField>
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
        public bool IsReadOnly { get; set; }

        public string OnFocus { get; set; }
        public string OnKeydown { get; set; }
        public string OnKeypress { get; set; }
        public string OnBlur { get; set; }
        public string OnChange { get; set; }
        public string OnClick { get; set; }
        public string ReferenceUrl { get; set; }
        public bool? IsImage { get; set; }
        public string MouseEnter { get; set; }
        public string MouseLeave { get; set; }
        public string CustomButtonHtml { get; set; }
        public string CustomButtonOnClick { get; set; }
        public ControlStructureModel Structure { get; set; }

        public int? ObjectApprovalStepTypeId { get; set; }

        protected void MappingBase<T>(Profile profile) where T : InputFieldInputModel
        {
            profile.CreateMap<InputField, T>()
                .ForMember(d => d.DataTypeId, m => m.MapFrom(f => (EnumDataType)f.DataTypeId))
                .ForMember(d => d.FormTypeId, m => m.MapFrom(f => (EnumFormType)f.FormTypeId))
                 .ForMember(d => d.Structure, m => m.MapFrom(f => string.IsNullOrEmpty(f.Structure) ? null : JsonConvert.DeserializeObject<ControlStructureModel>(f.Structure)))
                .ReverseMap()
                .ForMember(d => d.DataTypeId, m => m.MapFrom(f => (int)f.DataTypeId))
                .ForMember(d => d.FormTypeId, m => m.MapFrom(f => (int)f.FormTypeId))
                .ForMember(d => d.Structure, m => m.MapFrom(f => f.Structure == null ? string.Empty : JsonConvert.SerializeObject(f.Structure))); ;
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

    public class InputAreaFieldInputModel : IFieldData, IMapFrom<InputAreaField>
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
        public string ReferenceUrl { get; set; }
        public bool IsBatchSelect { get; set; }
        public string OnClick { get; set; }
        public string CustomButtonHtml { get; set; }
        public string CustomButtonOnClick { get; set; }
        public string MouseEnter { get; set; }
        public string MouseLeave { get; set; }
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
                RequireFilters == curField.RequireFilters &&
                ReferenceUrl == curField.ReferenceUrl &&
                IsBatchSelect == curField.IsBatchSelect &&
                OnClick == curField.OnClick &&
                CustomButtonHtml == curField.CustomButtonHtml &&
                CustomButtonOnClick == curField.CustomButtonOnClick &&
                MouseEnter == curField.MouseEnter &&
                MouseLeave == curField.MouseLeave;
        }
    }

    public class InputAreaFieldOutputFullModel : InputAreaFieldInputModel, IFieldExecData
    {
        public InputFieldOutputModel InputField { get; set; }

        private ExecCodeCombine<IFieldData> execCodeCombine;
        public InputAreaFieldOutputFullModel()
        {
            execCodeCombine = new ExecCodeCombine<IFieldData>(this);
        }

        public string OnFocusExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnFocus), InputField);
        public string OnKeydownExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnKeydown), InputField);
        public string OnKeypressExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnKeypress), InputField);
        public string OnBlurExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnBlur), InputField);
        public string OnChangeExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnChange), InputField);
        public string OnClickExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnClick), InputField);

        public string ReferenceUrlExec => execCodeCombine.GetExecCode(nameof(IFieldData.ReferenceUrl), InputField);

        public string MouseEnterExec => execCodeCombine.GetExecCode(nameof(IFieldData.MouseEnter), InputField);
        public string MouseLeaveExec => execCodeCombine.GetExecCode(nameof(IFieldData.MouseLeave), InputField);
        public string CustomButtonOnClickExec => execCodeCombine.GetExecCode(nameof(IFieldData.CustomButtonOnClick), InputField);
    }
}
