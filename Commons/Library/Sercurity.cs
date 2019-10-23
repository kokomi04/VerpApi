using Sodium;
using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Library
{
    public static class Sercurity
    {
        public static (string, string) GenerateHashPasswordHash(string pepper, string password)
        {
            var salt = Convert.ToBase64String(PasswordHash.ScryptGenerateSalt());
            var hash = PasswordHash.ScryptHashString(pepper + salt + password);
            return (salt, hash);
        }

        public static bool VerifyPasswordHash(string pepper, string salt, string password, string passwordHash)
        {
            return PasswordHash.ScryptHashStringVerify(passwordHash, pepper + salt + password);
        }
       
    }
}
