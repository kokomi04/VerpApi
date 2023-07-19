using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Organization.Model.Employee
{
    public class EmployeeFilterModel
    {
        public Clause Filters { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
