﻿
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input

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
        public bool IsHide { get; set; }
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
}
