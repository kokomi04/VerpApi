using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace Verp.Services.ReportConfig.Service
{
    public interface IReportConfigService
    {
        Task<ReportTypeViewModel> ReportTypeViewGetInfo(int reportTypeId, bool isConfig = false);

        Task<bool> ReportTypeViewUpdate(int reportTypeId, ReportTypeViewModel model);

        Task<int> ReportTypeGroupCreate(ReportTypeGroupModel model);

        Task<bool> ReportTypeGroupUpdate(int reportTypeGroupId, ReportTypeGroupModel model);

        Task<bool> ReportTypeGroupDelete(int reportTypeGroupId);

        Task<IList<ReportTypeGroupList>> ReportTypeGroupList();

        Task<PageData<ReportTypeListModel>> ReportTypes(string keyword, int page, int size, int? reportTypeGroupId = null);

        Task<ReportTypeModel> ReportType(int reportTypeId);

        Task<int> AddReportType(ReportTypeModel data);

        Task<int> UpdateReportType(int reportTypeId, ReportTypeModel data);

        Task<int> DeleteReportType(int reportTypeId);

        CipherFilterModel DecryptExtraFilter(CipherFilterModel cipherFilter);
    }
}
