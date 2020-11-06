using System;
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

    public class InputActionModel : InputActionSimpleProjectMappingModel
    {
       
        [Required(ErrorMessage = "Vui lòng nhập mã chức năng")]
        [MaxLength(45, ErrorMessage = "Mã chức năng quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chức năng chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string InputActionCode { get; set; }
        public string SqlAction { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
        //public EnumPosition Position { set; get; }
        public string JsVisible { get; set; }

        //protected void MappingBase<T>(Profile profile) where T : InputActionModel
        //{
        //    profile.CreateMap<InputAction, T>();
        //        //.ForMember(a => a.Position, m => m.MapFrom(a => (EnumPosition)a.Position))
        //        //.ReverseMap()
        //        //.ForMember(a => a.Position, m => m.MapFrom(a => (int)a.Position));

        //}

        //public void Mapping(Profile profile)
        //{
        //    MappingBase<InputActionModel>(profile);
        //}
    }
}
