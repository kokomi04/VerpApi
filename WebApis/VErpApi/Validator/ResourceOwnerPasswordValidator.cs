using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.WebApis.VErpApi.Validator
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UnAuthorizeMasterDBContext _masterDB;
        private readonly AppSetting _appSetting;

        private const int MAX_FAIL_ACCESS = 5;
        public ResourceOwnerPasswordValidator(UnAuthorizeMasterDBContext masterDB, IOptionsSnapshot<AppSetting> appSetting)
        {
            _masterDB = masterDB;
            _appSetting = appSetting?.Value;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (string.IsNullOrEmpty(context?.UserName) || string.IsNullOrEmpty(context.Password))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Bạn chưa nhập tên đăng nhập hoặc mật khẩu");
                return;
            }

            var strSubId = context.Request.Raw["subsidiary_id"];
            if (string.IsNullOrEmpty(strSubId))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Bạn chưa chọn công ty");
                return;
            }

            int subsidiaryId;
            if (!int.TryParse(strSubId, out subsidiaryId))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Thông tin công ty không hợp lệ");
                return;
            }

            var user = await GetUserByUsername(context.UserName, subsidiaryId);
            if (user == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Tên đăng nhập  {context.UserName} không tồn tại");
                return;
            }

            var userStatus = (EnumUserStatus)user.UserStatusId;
            if (userStatus != EnumUserStatus.Actived)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Tài khoản {context.UserName} chưa được kích hoạt hoặc đã bị khóa");
                return;
            }

            if (user.SubsidiaryId != subsidiaryId)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Thông tin công ty của tài khoản {context.UserName} không hợp lệ");
                return;
            }

            if (!IsValidCredential(user, context.Password))
            {
                await MaxFailedAccessAttempts(user, false);
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Mật khẩu không đúng. Số lần nhập sai {user.AccessFailedCount}/{MAX_FAIL_ACCESS}");
                return;
            }

            var customClaims = new List<Claim>
            {
                new Claim("userId", user.UserId+""),
                new Claim("clientId", context.Request.ClientId),
                new Claim("subsidiaryId", subsidiaryId+""),
            };

            await MaxFailedAccessAttempts(user, true);
            context.Result = new GrantValidationResult(
                context.UserName,
                authenticationMethod: "password",
                claims: customClaims,
                identityProvider: "local");
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
