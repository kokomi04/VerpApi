using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryPeriodModel: IMapFrom<SalaryPeriod>
    {
        public int SalaryPeriodId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? CensorByUserId { get; set; }
        public DateTime? CensorDatetimeUtc { get; set; }
        public EnumSalaryPeriodCensorStatus SalaryPeriodCensorStatusId { get; set; }
    }
}
