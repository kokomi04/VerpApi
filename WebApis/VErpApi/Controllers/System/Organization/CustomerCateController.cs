using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Organization.Service.Customer;
using VErp.Services.Organization.Model.Customer;
using System.Collections.Generic;
using VErp.Infrastructure.ApiCore.Attributes;
using System.Linq;
using VErp.Commons.GlobalObject;
using Newtonsoft.Json;
using VErp.Commons.Library.Model;
using VErp.Services.Master.Model.Config;
using VErp.Infrastructure.ApiCore.ModelBinders;

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