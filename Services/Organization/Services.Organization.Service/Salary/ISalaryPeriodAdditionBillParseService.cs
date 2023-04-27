using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionBillParseService
    {
        Task<CategoryNameModel> GetFieldDataMappingForParse(int salaryPeriodAdditionTypeId);

        Task<IList<SalaryPeriodAdditionBillEmployeeParseInfo>> ParseExcel(int salaryPeriodAdditionTypeId, ImportExcelMapping mapping, Stream stream);
    }
}
