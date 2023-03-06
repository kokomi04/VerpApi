using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryPeriodModel : IMapFrom<SalaryPeriod>
    {
        public int SalaryPeriodId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long FromDate { get; set; }
        public long ToDate { get; set; }

        public int? CheckedByUserId { get; set; }
        public long? CheckedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        public int? CensorByUserId { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public EnumSalaryPeriodCensorStatus SalaryPeriodCensorStatusId { get; set; }
    }

    public class SalaryPeriodGroupModel : IMapFrom<SalaryPeriodGroup>
    {
        public long SalaryPeriodGroupId { get; set; }
        public int SalaryPeriodId { get; set; }
        public int SalaryGroupId { get; set; }
        public long FromDate { get; set; }
        public long ToDate { get; set; }

        public int? CheckedByUserId { get; set; }
        public long? CheckedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        public int? CensorByUserId { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public EnumSalaryPeriodCensorStatus SalaryPeriodCensorStatusId { get; set; }
    }

    public class GroupSalaryEmployeeRequestModel
    {
        public long FromDate { get; set; }
        public long ToDate { get; set; }

    }

    public class GroupSalaryEmployeeModel : GroupSalaryEmployeeRequestModel
    {
        public IList<NonCamelCaseDictionary> Salaries { get; set; }
    }
}
