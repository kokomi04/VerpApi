using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class AbsenceTypeSymbolModel : IMapFrom<AbsenceTypeSymbol>
    {
        public int AbsenceTypeSymbolId { get; set; }
        public int NumericalOrder { get; set; }
        public string TypeSymbolDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số ngày nghỉ tối đa/tháng")]
        [Range(1, 31, ErrorMessage = "Số ngày nghỉ phải nằm trong khoảng từ 1 đến 31")]
        public int MaxOfDaysOffPerMonth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ký hiệu loại vắng")]
        [MaxLength(20, ErrorMessage = "Ký hiệu loại vắng quá dài")]
        public string SymbolCode { get; set; }
        public bool IsUsed { get; set; }
        public bool IsCounted { get; set; }
        public double SalaryRate { get; set; }
        public bool IsDefaultSystem { get; set; }
    }
}