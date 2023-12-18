using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.Report
{
    public interface IReportTypeHelperService
    {
        Task<PageData<ReportTypeBaseModel>> GetReportTypeSimpleList();
        Task<IList<ReportTypeGroupBaseModel>> GetGroups();
        
    }



    public class ReportTypeHelperService : IReportTypeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ReportTypeHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }
       
        public async Task<PageData<ReportTypeBaseModel>> GetReportTypeSimpleList()
        {
            return await _httpCrossService.Get<PageData<ReportTypeBaseModel>>($"api/report/internal/InternalReportType/simpleList");
        }

        public async Task<IList<ReportTypeGroupBaseModel>> GetGroups()
        {
            return await _httpCrossService.Get<List<ReportTypeGroupBaseModel>>($"api/report/internal/InternalReportType/groups");
        }

    }
}
