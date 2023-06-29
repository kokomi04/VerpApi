using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Organization;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Organization.Service.BusinessInfo;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Organization
{
    [Route("api/businessInfo")]

    public class BusinessInfoController : VErpBaseController
    {
        private readonly IBusinessInfoService _businessInfoService;
        private readonly IFileService _fileService;
        private readonly IFileProcessDataService _fileProcessDataService;

        public BusinessInfoController(IBusinessInfoService businessInfoService
            , IFileService fileService
            , IFileProcessDataService fileProcessDataService
            )
        {
            _businessInfoService = businessInfoService;
            _fileService = fileService;
            _fileProcessDataService = fileProcessDataService;
        }

        /// <summary>
        /// Lấy thông tin doanh nghiệp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [GlobalApi]
        [Route("")]
        public async Task<BusinessInfoModel> GetBusinessInfo()
        {
            return await _businessInfoService.GetBusinessInfo();
        }

        /// <summary>
        /// Cập nhật thông tin doanh nghiệp
        /// </summary>
        /// <param name="businessInfo"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<bool> UpdateCustomer([FromBody] BusinessInfoModel businessInfo)
        {
            var currentUserId = UserId;
            return await _businessInfoService.UpdateBusinessInfo(currentUserId, businessInfo);
        }

    }
}