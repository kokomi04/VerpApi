using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Service.Customer;
namespace VErpApi.Controllers.Stock.Internal
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
        [VErpAction(EnumAction.View)]
        [Route("")]
        public async Task<ServiceResult<PageData<CustomerListOutput>>> Get([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _customerService.GetList(keyword, customerStatusId, page, size, filters);
        }

        [HttpGet]
        [Route("{customerId}")]
        public async Task<ServiceResult<CustomerModel>> GetCustomerInfo([FromRoute] int customerId)
        {
            return await _customerService.GetCustomerInfo(customerId);
        }
    }
}