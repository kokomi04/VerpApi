using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryRefTableModel : IMapFrom<SalaryRefTable>
    {
        public int SalaryRefTableId { get; set; }
        public int SortOrder { get; set; }
        public string FromField { get; set; }
        [Required]
        [MinLength(1)]
        public string RefTableCode { get; set; }
        [Required]
        [MinLength(1)]
        public string RefTableField { get; set; }
    }
}
