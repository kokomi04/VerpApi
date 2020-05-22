using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;

namespace Verp.Services.ReportConfig.Service
{
    public interface IReportConfigService
    {
        Task<ReportTypeViewModel> ReportTypeViewGetInfo(int reportTypeId);

        Task<bool> ReportTypeViewUpdate(int reportTypeId, ReportTypeViewModel model);

        Task<int> ReportTypeGroupCreate(ReportTypeGroupModel model);

        Task<bool> ReportTypeGroupUpdate(int reportTypeGroupId, ReportTypeGroupModel model);

        Task<bool> ReportTypeGroupDelete(int reportTypeGroupId);

        Task<IList<ReportTypeGroupList>> ReportTypeGroupList();
    }
}
