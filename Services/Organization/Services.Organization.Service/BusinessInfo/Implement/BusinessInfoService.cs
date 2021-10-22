﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Verp.Resources.Organization.BussinessInfo;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using BusinessInfoEntity = VErp.Infrastructure.EF.OrganizationDB.BusinessInfo;

namespace VErp.Services.Organization.Service.BusinessInfo.Implement
{
    public class BusinessInfoService : IBusinessInfoService
    {
        private readonly OrganizationDBContext _organizationContext;

        private readonly ObjectActivityLogFacade _bussinessInfoActivityLog;

        public BusinessInfoService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<BusinessInfoService> logger
            , IActivityLogService activityLogService
            )
        {
            _organizationContext = organizationContext;        

            _bussinessInfoActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.BusinessInfo);
        }

        public async Task<BusinessInfoModel> GetBusinessInfo()
        {
            var businessInfo = await _organizationContext.BusinessInfo.FirstOrDefaultAsync();
            BusinessInfoModel result = new BusinessInfoModel(); ;
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

        public async Task<bool> UpdateBusinessInfo(int updatedUserId, BusinessInfoModel data)
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
                    UpdatedTime = DateTime.UtcNow,
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

            await _bussinessInfoActivityLog.LogBuilder(() => BussinessActivityLogMessage.Update)
                .MessageResourceFormatDatas(businessInfo.CompanyName)
                .ObjectId(businessInfo.BusinessInfoId)
                .JsonData(businessInfo.JsonSerialize())
                .CreateLog();

            return true;
        }
    }
}
