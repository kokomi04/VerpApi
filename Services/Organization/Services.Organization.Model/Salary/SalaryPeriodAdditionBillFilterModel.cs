using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Organization.Model.Salary
{   
    public class SalaryPeriodAdditionBillFilterModel
    {
        public string Keyword { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;

        public int? Year { get; set; }
        public int? Month { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }

        public Clause ColumnsFilters { get; set; }
    }

    public class SalaryPeriodAdditionBillsRequestModel : SalaryPeriodAdditionBillFilterModel
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class SalaryPeriodAdditionBillsExportModel : SalaryPeriodAdditionBillFilterModel
    {
        //public IList<string> FieldNames { get; set; }
    }
}
