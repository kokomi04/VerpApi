using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Customer;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Customer;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.System
{
    [Route("api/customers")]

    public class CustomerController : VErpBaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IFileService _fileService;
        private readonly IFileProcessDataService _fileProcessDataService;

        public CustomerController(ICustomerService customerService
            , IObjectGenCodeService objectGenCodeService
            , IFileService fileService
            , IFileProcessDataService fileProcessDataService
            )
        {
            _customerService = customerService;
            _objectGenCodeService = objectGenCodeService;
            _fileService = fileService;
            _fileProcessDataService = fileProcessDataService;
        }


        /// <summary>
        /// Lấy danh sách đối tác
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="customerStatusId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<CustomerListOutput>>> Get([FromQuery] string keyword, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size)
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
        public async Task<ServiceResult<int>> AddCustomer([FromBody] CustomerModel customer)
        {
            var updatedUserId = UserId;
            return await _customerService.AddCustomer(updatedUserId, customer);
        }

        /// <summary>
        /// Lấy thông tin đối tác
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{customerId}")]
        public async Task<ServiceResult<CustomerModel>> GetCustomerInfo([FromRoute] int customerId)
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
        public async Task<ServiceResult> UpdateCustomer([FromRoute] int customerId, [FromBody] CustomerModel customer)
        {
            var updatedUserId = UserId;
            return await _customerService.UpdateCustomer(updatedUserId, customerId, customer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{customerId}")]
        public async Task<ServiceResult> DeleteUnit([FromRoute] int customerId)
        {
            return await _customerService.DeleteCustomer(customerId);
        }

        /// <summary>
        /// Sinh mã đối tác
        /// </summary>     
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateCustomerCode")]
        public async Task<ServiceResult<string>> GenerateCustomerCode()
        {           
            return await _objectGenCodeService.GenerateCode(EnumObjectType.Customer);
        }

        /// <summary>
        /// Upload file dữ liệu khách hàng
        /// </summary>
        /// <param name="fileTypeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("File/{fileTypeId}")]
        public async Task<ServiceResult<long>> UploadExcelDataFile([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.Customer, fileTypeId, string.Empty, file);
        }

        /// <summary>
        /// Xử lý file - Đọc và cập nhật dữ liệu khách hàng
        /// </summary>
        /// <param name="fileId">Id của file đã được upload</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ImportCustomerData")]
        public async Task<ServiceResult> ImportCustomerData([FromBody] long fileId)
        {
            var currentUserId = UserId;
            return await _fileProcessDataService.ImportCustomerData(currentUserId, fileId);
        }
    }
}