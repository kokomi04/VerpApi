using ActivityLogDB;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using VErp.Commons.Constants;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.WebApis.VErpApi.Validator
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UnAuthorizeMasterDBContext _masterDB;
        private readonly UnAuthorizeOrganizationContext _organizationDBContext;
        private readonly Func<UnAuthorizActivityLogDBContext> _activityLogDBContext;
        private readonly AppSetting _appSetting;

        private const int MAX_FAIL_ACCESS = 5;
        public ResourceOwnerPasswordValidator(
            IHttpContextAccessor accessor
            , UnAuthorizeMasterDBContext masterDB
            , IOptionsSnapshot<AppSetting> appSetting
            , UnAuthorizeOrganizationContext organizationDBContext
            , Func<UnAuthorizActivityLogDBContext> activityLogDBContext)
        {
            _accessor = accessor;
            _masterDB = masterDB;
            _appSetting = appSetting?.Value;
            _organizationDBContext = organizationDBContext;
            _activityLogDBContext = activityLogDBContext;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var ipAddress = _accessor?.HttpContext?.Connection.RemoteIpAddress.ToString();

            string userName = context?.UserName;
            int? userId = null;
            string userAgent = _accessor?.HttpContext?.Request.Headers["User-Agent"];
            string message = null;

            var strSubId = context.Request.Raw[OAuthFormKeyConstants.SubsidiaryId];
            var subsidiayCode = "";
            var subsidiaryId = 0;

            if (string.IsNullOrWhiteSpace(strSubId))
            {
                var queryStrings = HttpUtility.ParseQueryString(_accessor?.HttpContext?.Request?.QueryString + "");
                if (queryStrings != null && queryStrings.AllKeys.Contains(OAuthFormKeyConstants.SubsidiaryCode))
                {
                    subsidiayCode = queryStrings[OAuthFormKeyConstants.SubsidiaryCode];
                }
            }
            else
            {
                if (!int.TryParse(strSubId, out subsidiaryId))
                {
                    message = "Thông tin công ty không hợp lệ";
                    await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                    context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                    return;
                }
            }


            if (subsidiaryId == 0 && string.IsNullOrEmpty(subsidiayCode))
            {
                message = "Bạn chưa chọn công ty";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }


            Subsidiary subdiaryInfo = null;
            if (subsidiaryId > 0)
            {
                subdiaryInfo = _organizationDBContext.Subsidiary.FirstOrDefault(s => s.SubsidiaryId == subsidiaryId);
            }
            else
            {
                subdiaryInfo = _organizationDBContext.Subsidiary.FirstOrDefault(s => s.SubsidiaryCode == subsidiayCode);
            }


            if (subdiaryInfo == null)
            {
                message = "Thông tin công ty không tồn tại";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }
            subsidiaryId = subdiaryInfo.SubsidiaryId;
            subsidiayCode = subdiaryInfo.SubsidiaryCode;
            strSubId = subdiaryInfo.SubsidiaryId + "";


            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(context.Password))
            {
                message = "Bạn chưa nhập tên đăng nhập hoặc mật khẩu";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }

            var user = await GetUserByUsername(userName, subdiaryInfo.SubsidiaryId);
            if (user == null)
            {
                message = $"Tên đăng nhập  {userName} không tồn tại";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }

            userId = user.UserId;

            var userStatus = (EnumUserStatus)user.UserStatusId;
            if (userStatus != EnumUserStatus.Actived)
            {
                message = $"Tài khoản {userName} chưa được kích hoạt hoặc đã bị khóa";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }

            if (user.SubsidiaryId != subdiaryInfo.SubsidiaryId)
            {
                message = $"Thông tin công ty của tài khoản {userName} không hợp lệ";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }

            if (!IsValidCredential(user, context.Password))
            {
                await MaxFailedAccessAttempts(user, false);
                message = $"Mật khẩu không đúng. Số lần nhập sai {user.AccessFailedCount}/{MAX_FAIL_ACCESS}";
                await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Failure, strSubId, message);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, message);
                return;
            }

            var customClaims = new List<Claim>
            {
                new Claim(UserClaimConstants.UserId, user.UserId+""),
                new Claim(UserClaimConstants.ClientId, context.Request.ClientId),
                new Claim(UserClaimConstants.SubsidiaryId, subdiaryInfo.SubsidiaryId+""),
                new Claim(UserClaimConstants.Developer, _appSetting.Developer?.IsDeveloper(userName, subdiaryInfo.SubsidiaryCode) == true? "1" :"0"),
            };
            await MaxFailedAccessAttempts(user, true);

            await CreateUserLoginLog(userId, userName, ipAddress, userAgent, EnumUserLoginStatus.Success, strSubId, "Đăng nhập thành công");
            context.Result = new GrantValidationResult(
                userName,
                authenticationMethod: "password",
                claims: customClaims,
                identityProvider: "local");
        }


        private async Task<bool> CreateUserLoginLog(int? userId,
            string userName,
            string ipAddress,
            string userAgent,
            EnumUserLoginStatus status,
            string strSubId,
            string message = null,
            string messageResourceName = null,
            string messageResourceFormatData = null)
        {
            var activityLogDB = _activityLogDBContext();
            var userLoginLog = new UserLoginLog
            {
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Status = (int)status,
                MessageTypeId = (int)EnumMessageType.UserLogin,
                MessageResourceName = messageResourceName,
                MessageResourceFormatData = messageResourceFormatData,
                Message = message,
                CreatedDatetimeUtc = DateTime.UtcNow,
                StrSubId = strSubId,
            };
            await activityLogDB.UserLoginLog.AddAsync(userLoginLog);
            await activityLogDB.SaveChangesAsync();
            return true;
        }

        private async Task<User> GetUserByUsername(string username, int subsidiayId)
        {
            var usernameHash = username.ToGuid();

            return await _masterDB.User.FirstOrDefaultAsync(u => u.UserNameHash == usernameHash && u.SubsidiaryId == subsidiayId && u.UserName == username);
        }

        private bool IsValidCredential(User user, string password)
        {
            return Sercurity.VerifyPasswordHash(_appSetting.PasswordPepper, user.PasswordSalt, password, user.PasswordHash);
        }

        private async Task<bool> MaxFailedAccessAttempts(User user, bool isAccess)
        {
            if (!isAccess)
            {
                user.AccessFailedCount += 1;
                user.UserStatusId = user.AccessFailedCount >= MAX_FAIL_ACCESS ? (int)EnumUserStatus.Locked : user.UserStatusId;
            }
            else
            {
                user.AccessFailedCount = 0;
            }

            await _masterDB.SaveChangesAsync();

            return true;
        }
    }
}
