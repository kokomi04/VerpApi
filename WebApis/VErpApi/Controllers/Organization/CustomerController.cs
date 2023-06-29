﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Service.Customer;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Organization
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


        [HttpPost]
        [Route("Search")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<PageData<CustomerListOutput>> Get([FromQuery] string keyword, [FromQuery] int? customerCateId, [FromQuery] IList<int> customerIds, [FromQuery] EnumCustomerStatus? customerStatusId, [FromQuery] int page, [FromQuery] int size, [FromBody] Clause filters)
        {
            return await _customerService.GetList(keyword, customerCateId, customerIds, customerStatusId, page, size, filters);
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("ExportList")]
        public async Task<IActionResult> ExportList([FromBody] CustomerListExportModel req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            var (stream, fileName, contentType) = await _customerService.ExportList(req.FieldNames, req.Keyword, req.CustomerCateId, req.CustomerIds, req.CustomerStatusId, req.Page, req.Size,req.ColumnsFilters);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }


        /// <summary>
        /// Lấy danh sách khách hàng theo Ids
        /// </summary>
        /// <param name="customerIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetByIds")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
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
        [GlobalApi]
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
            return await _customerService.UpdateCustomer(customerId, customer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{customerId}")]
        public async Task<bool> DeleteCustomer([FromRoute] int customerId)
        {
            return await _customerService.DeleteCustomer(customerId);
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
        public async Task<bool> ImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _customerService.ImportCustomerFromMapping(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("GetCustomerTopInUsed")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<CustomerInUsedInfo>> GetCustomerTopInUsed([FromBody] IList<int> customerIds)
        {
            return (await _customerService.GetCustomerTopInUsed(customerIds, false)).ToList();
        }
    }
}