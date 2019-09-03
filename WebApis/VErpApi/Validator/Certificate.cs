using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace VErp.WebApis.VErpApi.Validator
{
    static class Certificate
    {
        public static X509Certificate2 Get(string filePath, string password)
        {
            return new X509Certificate2(filePath, password);
        }        
    }
}