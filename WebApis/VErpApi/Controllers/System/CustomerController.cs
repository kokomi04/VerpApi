using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Customer;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Customer;

namespace VErpApi.Controllers.System
{
    [Route("api/customers")]

    public class CustomerController : VErpBaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        public CustomerController(ICustomerService customerService
            , IObjectGenCodeService objectGenCodeService
            )
        {
            _customerService = customerService;
            _objectGenCodeService = objectGenCodeService;
        }


        /// <summary>
        /// Lấy danh sách đối tác
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<CustomerListOutput>>> Get([FromQuery] string keyword, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _customerService.GetList(keyword, customerStatusId, page, size);
        }

        /// <summary>
        /// Thêm mới đối tác
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddCustomer([FromBody] CustomerModel customer)
        {
            return await _customerService.AddCustomer(customer);
        }

        /// <summary>
        /// Lấy thông tin đối tác
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{customerId}")]
        public async Task<ApiResponse<CustomerModel>> GetCustomerInfo([FromRoute] int customerId)
        {
            return await _customerService.GetCustomerInfo(customerId);
        }

        /// <summary>
        /// Cập nhật thông tin đối tác
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{customerId}")]
        public async Task<ApiResponse> UpdateCustomer([FromRoute] int customerId, [FromBody] CustomerModel customer)
        {
            return await _customerService.UpdateCustomer(customerId, customer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{customerId}")]
        public async Task<ApiResponse> DeleteUnit([FromRoute] int customerId)
        {
            return await _customerService.DeleteCustomer(customerId);
        }

        /// <summary>
        /// Sinh mã đối tác
        /// </summary>     
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateCustomerCode")]
        public async Task<ApiResponse<string>> GenerateCustomerCode()
        {           
            return await _objectGenCodeService.GenerateCode(EnumObjectType.Customer);
        }
    }
}