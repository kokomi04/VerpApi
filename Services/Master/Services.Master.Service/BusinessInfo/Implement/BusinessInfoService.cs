using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.BusinessInfo;
using BusinessInfoEntity = VErp.Infrastructure.EF.MasterDB.BusinessInfo;

namespace VErp.Services.Master.Service.BusinessInfo.Implement
{
    public class BusinessInfoService : IBusinessInfoService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public BusinessInfoService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<BusinessInfoService> logger
            , IActivityLogService activityLogService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ApiResponse<BusinessInfoModel>> GetBusinessInfo()
        {
            var businessInfo = await _masterContext.BusinessInfo.FirstOrDefaultAsync();
            BusinessInfoModel result = null;
            if (businessInfo != null)
            {
                result = new BusinessInfoModel
                {
                    CompanyName = businessInfo.CompanyName,
                    LegalRepresentative = businessInfo.LegalRepresentative,
                    Address = businessInfo.Address,
                    TaxIdNo = businessInfo.TaxIdNo,
                    Website = businessInfo.Website,
                    PhoneNumber = businessInfo.PhoneNumber,
                    Email = businessInfo.Email,
                    LogoFileId = businessInfo.LogoFileId
                };
            }

            return result;
        }

        public async Task<Enum> UpdateBusinessInfo(int updatedUserId, BusinessInfoModel data)
        {
            var businessInfo = await _masterContext.BusinessInfo.FirstOrDefaultAsync();
            if (businessInfo == null)
            {
                // Tạo mới
                businessInfo = new BusinessInfoEntity
                {
                    CompanyName = data.CompanyName,
                    LegalRepresentative = data.LegalRepresentative,
                    Address = data.Address,
                    TaxIdNo = data.TaxIdNo,
                    Website = data.Website,
                    PhoneNumber = data.PhoneNumber,
                    Email = data.Email,
                    LogoFileId = data.LogoFileId,
                    CreatedTime = DateTime.Now,
                    UpdatedUserId = updatedUserId
                };
                _masterContext.BusinessInfo.Add(businessInfo);
            }
            else
            {
                // Update
                businessInfo.CompanyName = data.CompanyName;
                businessInfo.LegalRepresentative = data.LegalRepresentative;
                businessInfo.Address = data.Address;
                businessInfo.TaxIdNo = data.TaxIdNo;
                businessInfo.Website = data.Website;
                businessInfo.PhoneNumber = data.PhoneNumber;
                businessInfo.Email = data.Email;
                businessInfo.LogoFileId = data.LogoFileId;
                businessInfo.UpdatedTime = DateTime.Now;
                businessInfo.UpdatedUserId = updatedUserId;
            }
            await _masterContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.BusinessInfo, businessInfo.BusinessInfoId, $"Cập nhật thông tin doanh nghiệp {businessInfo.CompanyName}", data.JsonSerialize());
            return GeneralCode.Success;
        }
    }
}
