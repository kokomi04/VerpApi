using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.Attributes;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class OvertimeLevelModel : IMapFrom<OvertimeLevel>
    {
        public int OvertimeLevelId { get; set; }
        public int NumericalOrder { get; set; }

        [Min(1, ErrorMessage = "Tỷ lệ hưởng lương phải lớn hơn hoặc bằng 1")]
        public decimal OvertimeRate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ký hiệu mức tăng ca")]
        [MaxLength(20, ErrorMessage = "Ký hiệu mức tăng ca quá dài")]
        [Unique<OrganizationDBContext, OvertimeLevel>(ErrorMessage = "Ký hiệu mức tăng ca đã tồn tại")]
        public string OvertimeCode { get; set; }

        [MaxLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả mức tăng ca")]
        public string Description { get; set; }

        public int OvertimePriority { get; set; }
    }
}