using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Organization.Model.Employee
{

    public class EmplyeeRequestFilterModel
    {
        public Clause Filters { get; set; }
        public EmplyeeRequestFilterModel(Clause filters) {
            Filters = filters;
        }
    }
    public class EmployeeFilterModel : EmplyeeRequestFilterModel
    {
        public EmployeeFilterModel(Clause filters): base(filters)
        {

        }
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class EmployeePeriodGroupRequestExportModel : EmplyeeRequestFilterModel
    {
        public EmployeePeriodGroupRequestExportModel(Clause filters): base(filters)
        {

        }
        public IList<string> FieldNames { get; set; }
        public int SalaryPeriodId { get; set; }
        public int SalaryGroupId { get; set; }
        public IList<string> GroupFields { get; set; }
    }

}
