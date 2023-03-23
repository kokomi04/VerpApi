using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryPeriodModel : IMapFrom<SalaryPeriod>
    {
        public int SalaryPeriodId { get; set; }
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
        [Required]
        [Range(2010, 2100)]
        public int Year { get; set; }
        [Required]
        public long FromDate { get; set; }
        [Required]
        public long ToDate { get; set; }
  
    }

    public class SalaryPeriodInfo : SalaryPeriodModel
    {       
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
    }


    public class SalaryPeriodGroupInfo : SalaryPeriodGroupModel
    {       
        public int? CheckedByUserId { get; set; }
        public long? CheckedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        public int? CensorByUserId { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public EnumSalaryPeriodCensorStatus SalaryPeriodCensorStatusId { get; set; }
        public bool IsSalaryDataCreated { get; set; }
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
