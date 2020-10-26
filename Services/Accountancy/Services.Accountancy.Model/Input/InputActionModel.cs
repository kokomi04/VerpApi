using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputActionSimpleModel: IMapFrom<InputAction>
    {
        public int InputTypeId { get; set; }
        public int InputActionId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chức năng")]
        [MaxLength(256, ErrorMessage = "Tên chức năng quá dài")]
        public string Title { get; set; }
        public int SortOrder { get; set; }
    }
    public class InputActionModel : InputActionSimpleModel
    {
       
        [Required(ErrorMessage = "Vui lòng nhập mã chức năng")]
        [MaxLength(45, ErrorMessage = "Mã chức năng quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chức năng chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string InputActionCode { get; set; }
        public string SqlAction { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
    }
}
