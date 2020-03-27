using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.BusinessInfo;
using BusinessInfoEntity = VErp.Infrastructure.EF.OrganizationDB.BusinessInfo;

namespace VErp.Services.Organization.Service.BusinessInfo.Implement
{
    public class BusinessInfoService : IBusinessInfoService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public BusinessInfoService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<BusinessInfoService> logger
            , IActivityLogService activityLogService
            )
        {
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<BusinessInfoModel>> GetBusinessInfo()
        {
            var businessInfo = await _organizationContext.BusinessInfo.FirstOrDefaultAsync();
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
            var businessInfo = await _organizationContext.BusinessInfo.FirstOrDefaultAsync();
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
                    CreatedTime = DateTime.UtcNow,
                    UpdatedUserId = updatedUserId
                };
                _organizationContext.BusinessInfo.Add(businessInfo);
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
                businessInfo.UpdatedTime = DateTime.UtcNow;
                businessInfo.UpdatedUserId = updatedUserId;
            }
            await _organizationContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.BusinessInfo, businessInfo.BusinessInfoId, $"Cập nhật thông tin doanh nghiệp {businessInfo.CompanyName}", data.JsonSerialize());
            return GeneralCode.Success;
        }
    }
}
