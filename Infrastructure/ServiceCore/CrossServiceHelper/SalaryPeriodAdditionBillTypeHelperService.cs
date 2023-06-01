using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.Hr.Salary;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
   
    public interface ISalaryPeriodAdditionBillTypeHelperService
    {
        Task<IEnumerable<ISalaryPeriodAddtionTypeBase>> ListTypes();
    }


    public class SalaryPeriodAdditionBillTypeHelperService : ISalaryPeriodAdditionBillTypeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public SalaryPeriodAdditionBillTypeHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<IEnumerable<ISalaryPeriodAddtionTypeBase>> ListTypes()
        {

            return await _httpCrossService.Get<List<SalaryPeriodAddtionTypeBase>>($"api/internal/InternalSalaryPeriodAdditionType");
        }       
    }
}
