using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Master.Service.BusinessInfo;
using VErp.Services.Master.Model.BusinessInfo;
using VErp.Infrastructure.ServiceCore.Model;

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
        [Route("")]
        public async Task<ServiceResult<BusinessInfoModel>> GetBusinessInfo()
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
        public async Task<ServiceResult> UpdateCustomer([FromBody] BusinessInfoModel businessInfo)
        {
            var currentUserId = UserId;
            return await _businessInfoService.UpdateBusinessInfo(currentUserId, businessInfo);
        }

    }
}