using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using Newtonsoft.Json;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject.DynamicBill;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.HrConfig
{
    public class HrFieldInputModel : IFieldData, IMapFrom<HrField>
    {
        public int HrAreaId { get; set; }
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

        public ControlStructureModel Structure { get; set; }
        protected void MappingBase<T>(Profile profile) where T : HrFieldInputModel
        {
            profile.CreateMap<HrField, T>()
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
            MappingBase<HrFieldInputModel>(profile);
        }

    }

    public class HrFieldOutputModel : HrFieldInputModel
    {
        public int HrFieldId { get; set; }
        public new void Mapping(Profile profile)
        {
            MappingBase<HrFieldOutputModel>(profile);
        }
    }

    public class HrAreaFieldInputModel : IFieldData, IMapFrom<HrAreaField>
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int HrFieldId { get; set; }
        public int? HrAreaFieldId { get; set; }
        public int HrAreaId { get; set; }
        public int HrTypeId { get; set; }
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
        public bool Compare(HrAreaField curField)
        {
            return !curField.IsDeleted &&
                HrAreaId == curField.HrAreaId &&
                HrFieldId == curField.HrFieldId &&
                HrTypeId == curField.HrTypeId &&
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
                OnClick == curField.OnClick;
        }
    }

    public class HrAreaFieldOutputFullModel : HrAreaFieldInputModel, IFieldExecData
    {
        public HrFieldOutputModel HrField { get; set; }

        private ExecCodeCombine<IFieldData> execCodeCombine;
        public HrAreaFieldOutputFullModel()
        {
            execCodeCombine = new ExecCodeCombine<IFieldData>(this);
        }

        public string OnFocusExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnFocus), HrField);
        public string OnKeydownExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnKeydown), HrField);
        public string OnKeypressExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnKeypress), HrField);
        public string OnBlurExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnBlur), HrField);
        public string OnChangeExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnChange), HrField);
        public string OnClickExec => execCodeCombine.GetExecCode(nameof(IFieldData.OnClick), HrField);

        public string ReferenceUrlExec => execCodeCombine.GetExecCode(nameof(IFieldData.ReferenceUrl), HrField);
    }
}
