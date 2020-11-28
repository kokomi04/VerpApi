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

namespace VErpApi.Controllers.System
{
    [Route("api/customers")]

    public class CustomerController : VErpBaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IGenCodeConfigService _genCodeConfigService;
        private readonly IFileService _fileService;
        private readonly IFileProcessDataService _fileProcessDataService;

        public CustomerController(ICustomerService customerService
            , IObjectGenCodeService objectGenCodeService
            , IGenCodeConfigService genCodeConfigService
            , IFileService fileService
            , IFileProcessDataService fileProcessDataService
            )
        {
            _customerService = customerService;
            _objectGenCodeService = objectGenCodeService;
            _genCodeConfigService = genCodeConfigService;
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
        public async Task<PageData<CustomerListOutput>> Get([FromQuery] string keyword, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _customerService.GetList(keyword, customerStatusId, page, size);
        }

        /// <summary>
        /// Lấy danh sách khách hàng theo Ids
        /// </summary>
        /// <param name="customerIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetByIds")]
        [VErpAction(EnumAction.View)]
        public async Task<IList<CustomerListOutput>> GetListByIds([FromBody] IList<int> customerIds)
        {
            return (await _customerService.GetListByIds(customerIds)).ToList();
        }

        /// <summary>
        /// Thêm mới đối tác
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<int> AddCustomer([FromBody] CustomerModel customer)
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
        public async Task<CustomerModel> GetCustomerInfo([FromRoute] int customerId)
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
        public async Task<bool> UpdateCustomer([FromRoute] int customerId, [FromBody] CustomerModel customer)
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
        public async Task<bool> DeleteUnit([FromRoute] int customerId)
        {
            return await _customerService.DeleteCustomer(customerId);
        }

        /// <summary>
        /// Sinh mã đối tác
        /// </summary>     
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateCustomerCode")]
        public async Task<CustomCodeModel> GenerateCustomerCode()
        {
            var currentConfig = await _objectGenCodeService.GetCurrentConfig(EnumObjectType.Customer, EnumObjectType.Customer, 0);
            return await _genCodeConfigService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
        }

        /// <summary>
        /// Upload file dữ liệu khách hàng
        /// </summary>
        /// <param name="fileTypeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("File/{fileTypeId}")]
        public async Task<long> UploadExcelDataFile([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
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
        public async Task<bool> ImportCustomerData([FromBody] long fileId)
        {
            var currentUserId = UserId;
            return await _fileProcessDataService.ImportCustomerData(currentUserId, fileId);
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            return _customerService.GetCustomerFieldDataForMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _customerService.ImportCustomerFromMapping(JsonConvert.DeserializeObject<ImportExcelMapping>(mapping), file.OpenReadStream()).ConfigureAwait(true);
        }
    }
}