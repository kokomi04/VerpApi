using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionBillImportService
    {
        Task<CategoryNameModel> GetFieldDataForMapping(int salaryPeriodAdditionTypeId);
        Task<bool> Import(int salaryPeriodAdditionTypeId, ImportExcelMapping mapping, Stream stream);
    }
}
