﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using AutoMapper;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputActionSimpleProjectMappingModel : InputActionSimpleModel, IMapFrom<InputAction>
    {

    }
    public class InputActionUseModel : InputActionSimpleProjectMappingModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã chức năng")]
        [MaxLength(45, ErrorMessage = "Mã chức năng quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chức năng chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string InputActionCode { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
        public string JsVisible { get; set; }
    }

    public class InputActionModel : InputActionUseModel
    {
        public string SqlAction { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputAction, InputActionModel>()
                .ForMember(d => d.ActionTypeId, s => s.MapFrom(m => (EnumActionType?)m.ActionTypeId))
                .ReverseMap()
                .ForMember(d => d.ActionTypeId, s => s.MapFrom(m => (int?)m.ActionTypeId));
        }
    }
}
