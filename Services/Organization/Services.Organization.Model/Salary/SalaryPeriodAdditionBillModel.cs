using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Commons.Library.Model;
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

        [Display(Name = "Số chứng từ", GroupName = "TT chung")]      
        [MaxLength(128)]
        public string BillCode { get; set; }
        [Required]
        [Range(2010, 2100)]
        [Display(Name = "Năm", GroupName = "TT chung")]
        public int? Year { get; set; }
        [Required]
        [Range(1, 12)]
        [Display(Name = "Tháng", GroupName = "TT chung")]
        public int? Month { get; set; }
        [MaxLength(512)]
        [Display(Name = "Nội dung", GroupName = "TT chung")]
        public string Content { get; set; }

        [Display(Name = "Ngày chứng từ", GroupName = "TT chung")]
        [Required]
        public long? Date { get; set; }
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
        [FieldDataIgnore]
        public long? SalaryPeriodAdditionBillEmployeeId { get; set; }

        [Required]
        [Display(Name = "Nhân viên", GroupName = "Chi tiết")]
        public long EmployeeId { get; set; }

        [Display(Name = "Ghi chú", GroupName = "Chi tiết")]
        public string Description { get; set; }

        [FieldDataIgnore]
        public NonCamelCaseDictionary<decimal?> Values { get; set; }
    }

    public class SalaryPeriodAdditionBillEmployeeParseInfo: SalaryPeriodAdditionBillEmployeeModel
    {
        public NonCamelCaseDictionary EmployeeInfo { get; set; }
    }

}
