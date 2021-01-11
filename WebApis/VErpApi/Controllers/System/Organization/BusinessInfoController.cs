using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.BusinessInfo;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore.Attributes;

namespace VErpApi.Controllers.System
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