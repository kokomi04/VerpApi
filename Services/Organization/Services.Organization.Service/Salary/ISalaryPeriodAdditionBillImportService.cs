using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionBillExportService
    {
        Task<(Stream stream, string fileName, string contentType)> Export(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionBillsExportModel req);
    }
}
