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

        public static string GetPhysicalFilePath(this string filePath, AppSetting appSetting)
        {
            filePath = filePath.Replace('\\', '/');

            while (filePath.StartsWith('.')|| filePath.StartsWith('/'))
            {
                filePath = filePath.TrimStart('/').TrimStart('.');
            }

            return appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\') + "/" + filePath;
        }
    }
}
