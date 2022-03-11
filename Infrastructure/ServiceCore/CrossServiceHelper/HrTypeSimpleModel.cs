﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public class HrTypeSimpleModel
    {
        public int HrTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chứng từ")]
        [MaxLength(256, ErrorMessage = "Tên chứng từ quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã chứng từ")]
        [MaxLength(45, ErrorMessage = "Mã chứng từ quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chứng từ chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string HrTypeCode { get; set; }

        public int SortOrder { get; set; }
        public int? HrTypeGroupId { get; set; }
        
        public IList<HrAreaFieldSimpleModel> AreaFields { get; set; }
    }


    public class HrAreaFieldSimpleModel
    {
        public int HrAreaId { get; set; }
        public string HrAreaTitle { get; set; }
        public int HrAreaFieldId { get; set; }
        public string HrAreaFieldTitle { get; set; }
        public int HrFieldId { get; set; }
        public EnumFormType FormTypeId { get; set; }
    }
}
