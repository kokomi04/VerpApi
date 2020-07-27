﻿
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input

{
    public class InputTypeModel: IMapFrom<InputType>
    {
        public InputTypeModel()
        {
        }

        public int InputTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chứng từ")]
        [MaxLength(256, ErrorMessage = "Tên chứng từ quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã chứng từ")]
        [MaxLength(45, ErrorMessage = "Mã chứng từ quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chứng từ chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string InputTypeCode { get; set; }

        public int SortOrder { get; set; }
        public int? InputTypeGroupId { get; set; }
        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }

        public InputTypeMenuStyle MenuStyle { get; set; }
    }

    public class InputTypeFullModel : InputTypeModel
    {
        public InputTypeFullModel()
        {
            InputAreas = new List<InputAreaModel>();
        }
        public ICollection<InputAreaModel> InputAreas { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputType, InputTypeFullModel>()
                .ForMember(dest => dest.InputAreas, opt => opt.MapFrom(src => src.InputArea));
        }
    }

    public class InputTypeMenuStyle
    {
        public int? ParentId { get; set; }
        public int ModuleId { get; set; }
        public string MenuName { get; set; }
        public string UrlFormat { get; set; }
        public string ParamFormat { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsDisabled { get; set; }
    }
}