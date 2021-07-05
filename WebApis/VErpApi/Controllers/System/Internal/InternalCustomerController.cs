using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Service.Customer;
namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalCustomerController : CrossServiceBaseController
    {
        private readonly ICustomerService _customerService;
        public InternalCustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<CustomerListBasicOutput>> Get([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] IList<int> customerIds, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            var r = await _customerService.GetList(keyword, customerIds, customerStatusId, page, size, filters);
            return (r.List.Select(c => new CustomerListBasicOutput() { CustomerId = c.CustomerId, CustomerCode = c.CustomerCode, CustomerName = c.CustomerName }).ToList(), r.Total);
        }

        [HttpGet]
        [Route("{customerId}")]
        public async Task<CustomerModel> GetCustomerInfo([FromRoute] int customerId)
        {
            return await _customerService.GetCustomerInfo(customerId);
        }
    }
}