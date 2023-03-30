using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryPeriodAdditionFieldModel : IMapFrom<SalaryPeriodAdditionField>
    {
        [MinLength(1)]
        [MaxLength(128)]
        [Required]
        public string FieldName { get; set; }
        [MinLength(1)]
        [MaxLength(128)]
        [Required]
        public string Title { get; set; }
        [Required]
        public int DecimalPlace { get; set; }
            
    }

    public class SalaryPeriodAdditionFieldInfo : SalaryPeriodAdditionFieldModel
    {
        public int SalaryPeriodAdditionFieldId { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }
}
