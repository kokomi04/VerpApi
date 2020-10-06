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
        private readonly MasterDBContext _masterDB;
        private readonly OrganizationDBContext _organizationDB;
        private readonly AppSetting _appSetting;

        private const int MAX_FAIL_ACCESS = 5;
        public ResourceOwnerPasswordValidator(MasterDBContext masterDB, OrganizationDBContext organizationDB, IOptionsSnapshot<AppSetting> appSetting)
        {
            _masterDB = masterDB;
            _organizationDB = organizationDB;
            _appSetting = appSetting?.Value;
        }
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (string.IsNullOrEmpty(context?.UserName) || string.IsNullOrEmpty(context.Password))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Bạn chưa nhập tên đăng nhập hoặc mật khẩu");
                return;
            }

            var user = await GetUserByUsername(context.UserName);
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

            var subsidiaryId = context.Request.Raw["subsidiary_id"];
            if (string.IsNullOrEmpty(subsidiaryId))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Bạn chưa chọn công ty");
                return;
            }

            var sub = await GetSubsidiaryByUserId(subsidiaryId);
            if (sub == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Công ty không tồn tại");
                return;
            }

            if (!IsValidSubsidiary(user.UserId, sub.SubsidiaryId))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Tài khoản {context.UserName} không thuộc công ty {sub.SubsidiaryName}");
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
                new Claim("subsidiaryId", subsidiaryId),
            };

            await MaxFailedAccessAttempts(user, true);
            context.Result = new GrantValidationResult(
                context.UserName,
                authenticationMethod: "password",
                claims: customClaims,
                identityProvider: "local");
        }

        private async Task<User> GetUserByUsername(string username)
        {
            var usernameHash = username.ToGuid();

            return await _masterDB.User.FirstOrDefaultAsync(u => u.UserNameHash == usernameHash);
        }

        private async Task<Subsidiary> GetSubsidiaryByUserId(string subsidiaryId)
        {
            int subId;
            _ = int.TryParse(subsidiaryId, out subId);
            return await _organizationDB.Subsidiary.FirstOrDefaultAsync(s => s.SubsidiaryId == subId);
        }

        private bool IsValidSubsidiary(int userId, int subsidiaryId)
        {
            return _organizationDB.EmployeeSubsidiary
                .Where(x => x.UserId == userId)
                .Any(s => s.SubsidiaryId == subsidiaryId);
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
