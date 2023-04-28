using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Verp.Services.ReportConfig.Service
{
    public interface IFilterSettingReportService
    {
        Task<Dictionary<int, object>> Get(int reportTypeId);
        Task<bool> Update(int reportTypeId, Dictionary<int, object> fieldValues);
    }
}
