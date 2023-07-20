﻿using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class AbsenceTypeSymbolModel : IMapFrom<AbsenceTypeSymbol>
    {
        public int AbsenceTypeSymbolId { get; set; }
        public int NumericalOrder { get; set; }

        [MaxLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string TypeSymbolDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số ngày nghỉ tối đa/tháng")]
        [Range(1, 31, ErrorMessage = "Số ngày nghỉ phải nằm trong khoảng từ 1 đến 31")]
        public int MaxOfDaysOffPerMonth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ký hiệu loại vắng")]
        [MaxLength(20, ErrorMessage = "Ký hiệu loại vắng quá dài")]
        [Unique<OrganizationDBContext, AbsenceTypeSymbol>(ErrorMessage = "Ký hiệu loại vắng đã tồn tại")]
        public string SymbolCode { get; set; }
        public bool IsUsed { get; set; }
        public bool IsCounted { get; set; }

        [Range(0, 1, ErrorMessage = "Tỷ lệ hưởng lương phải nằm trong khoảng từ 0 đến 1")]
        public double SalaryRate { get; set; }
        public bool IsDefaultSystem { get; set; }
    }
}