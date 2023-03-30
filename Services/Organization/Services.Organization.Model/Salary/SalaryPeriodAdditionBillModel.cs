using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    internal interface ISalaryPeriodAdditionBillModel
    {
        IList<SalaryPeriodAdditionBillEmployeeModel> Details { get; set; }
    }

    internal interface ISalaryPeriodAdditionBillList
    {
        long SalaryPeriodAdditionBillId { get; set; }
        int CreatedByUserId { get; set; }
        long CreatedDatetimeUtc { get; set; }
        int UpdatedByUserId { get; set; }
        long UpdatedDatetimeUtc { get; set; }
    }

    public class SalaryPeriodAdditionBillBase : IMapFrom<SalaryPeriodAdditionBill>
    {
        [MaxLength(128)]
        public string BillCode { get; set; }
        [Required]
        [Range(2010, 2100)]
        public int Year { get; set; }
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
        [MaxLength(512)]
        public string Description { get; set; }
    }

    public class SalaryPeriodAdditionBillModel : SalaryPeriodAdditionBillBase, ISalaryPeriodAdditionBillModel
    {
        public IList<SalaryPeriodAdditionBillEmployeeModel> Details { get; set; }
    }

    public class SalaryPeriodAdditionBillList : SalaryPeriodAdditionBillBase, ISalaryPeriodAdditionBillList
    {
        public long SalaryPeriodAdditionBillId { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }

    public class SalaryPeriodAdditionBillInfo : SalaryPeriodAdditionBillList, ISalaryPeriodAdditionBillModel
    {
        public IList<SalaryPeriodAdditionBillEmployeeModel> Details { get; set; }
    }

    public class SalaryPeriodAdditionBillEmployeeModel : IMapFrom<SalaryPeriodAdditionBillEmployee>
    {
        public long? SalaryPeriodAdditionBillEmployeeId { get; set; }
        public long EmployeeId { get; set; }
        public string Description { get; set; }
        public NonCamelCaseDictionary<decimal?> Values { get; set; }
    }

}
