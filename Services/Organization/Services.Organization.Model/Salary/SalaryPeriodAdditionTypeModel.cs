using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Hr.Salary;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryPeriodAdditionTypeModel : IMapFrom<SalaryPeriodAdditionType>
    {
        [MinLength(1)]
        [MaxLength(128)]
        [Required]
        public string Title { get; set; }
        [MaxLength(512)]
        public string Description { get; set; }
        public bool IsActived { get; set; }
        [Required]
        public IList<SalaryPeriodAdditionTypeFieldModel> Fields { get; set; }
    }

    public class SalaryPeriodAdditionTypeInfo : SalaryPeriodAdditionTypeModel, ISalaryPeriodAddtionTypeBase
    {
        public int SalaryPeriodAdditionTypeId { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }

    public class SalaryPeriodAdditionTypeFieldModel : IMapFrom<SalaryPeriodAdditionTypeField>
    {
        public int SalaryPeriodAdditionFieldId { get; set; }
        public int SortOrder { get; set; }
    }
}
