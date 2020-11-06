using Microsoft.AspNetCore.DataProtection;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Services.Stock.Service.FileResources
{
    public static class FileStoreUltils
    {
        public static string EncryptFileKey(this string input, IDataProtectionProvider dataProtectionProvider, AppSetting appSetting)
        {
            var protector = dataProtectionProvider.CreateProtector(appSetting.FileUrlEncryptPepper);
            return protector.Protect(input);
        }

        public static string DecryptFileKey(this string cipherText, IDataProtectionProvider dataProtectionProvider, AppSetting appSetting)
        {
            var protector = dataProtectionProvider.CreateProtector(appSetting.FileUrlEncryptPepper);
            return protector.Unprotect(cipherText);
        }

    }
}
