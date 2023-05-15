using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Hr.Salary
{
    public interface ISalaryPeriodAddtionTypeBase
    {
        int SalaryPeriodAdditionTypeId { get; }
        string Title { get; }
    }
    public class SalaryPeriodAddtionTypeBase : ISalaryPeriodAddtionTypeBase
    {
        public int SalaryPeriodAdditionTypeId { get; set; }
        public string Title { get; set; }
    }
}
