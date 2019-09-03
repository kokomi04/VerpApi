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

namespace VErp.WebApis.VErpApi.Validator
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly MasterDBContext _masterDB;
        private readonly AppSetting _appSetting;
        public ResourceOwnerPasswordValidator(MasterDBContext masterDB, IOptionsSnapshot<AppSetting> appSetting)
        {
            _masterDB = masterDB;
            _appSetting = appSetting.Value;
        }
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (string.IsNullOrEmpty(context.UserName) || string.IsNullOrEmpty(context.Password))
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
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Tài khoản {context.UserName} chưa được kích hoạt");
                return;
            }

            if (!IsValidCredential(user, context.Password))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, $"Mật khẩu không đúng");
                return;
            }

            var customClaims = new List<Claim>
            {
                new Claim("userId", user.UserId.ToString()),
                new Claim("clientId", context.Request.ClientId),
            };

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
        private bool IsValidCredential(User user, string password)
        {
            return Sercurity.VerifyPasswordHash(_appSetting.PasswordPepper, user.PasswordSalt, password, user.PasswordHash);
        }
    }
}
