using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Service.Customer;

namespace VErpApi.Controllers.System
{
    [Route("api/customerCate")]

    public class CustomerCateController : VErpBaseController
    {
        private readonly ICustomerCateService _customerCateService;

        public CustomerCateController(ICustomerCateService customerService)
        {
            _customerCateService = customerService;
        }

        [Route("")]
        [HttpGet]
        public async Task<IList<CustomerCateModel>> Get()
        {
            return await _customerCateService.GetList();
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Create([FromBody] CustomerCateModel model)
        {
            var updatedUserId = UserId;
            return await _customerCateService.CreateCustomerCate(model);
        }

        [HttpGet]
        [Route("{customerCateId}")]
        public async Task<CustomerCateModel> GetCustomerInfo([FromRoute] int customerCateId)
        {
            return await _customerCateService.GetInfo(customerCateId);
        }

        [HttpPut]
        [Route("{customerCateId}")]
        public async Task<bool> UpdateCustomer([FromRoute] int customerCateId, [FromBody] CustomerCateModel model)
        {
            return await _customerCateService.UpdateCustomerCate(customerCateId, model);
        }

        [HttpDelete]
        [Route("{customerCateId}")]
        public async Task<bool> DeleteUnit([FromRoute] int customerCateId)
        {
            return await _customerCateService.DeleteCustomerCate(customerCateId);
        }
    }
}