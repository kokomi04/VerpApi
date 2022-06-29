using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Service.BusinessInfo;
using VErp.Services.Organization.Service.Customer;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalBussinessController : CrossServiceBaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IBusinessInfoService _businessInfoService;

        public InternalBussinessController(ICustomerService customerService, IBusinessInfoService businessInfoService)
        {
            _customerService = customerService;
            _businessInfoService = businessInfoService;
        }

        /*
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<CustomerListOutput>> Get([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int? customerCateId, [FromQuery] IList<int> customerIds, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _customerService.GetList(keyword, customerCateId, customerIds, customerStatusId, page, size, filters);
        }
        */

        [HttpGet]
        [Route("businessInfo")]
        public async Task<BusinessInfoModel> GetBusinessInfo()
        {
            return await _businessInfoService.GetBusinessInfo();
        }
    }
}