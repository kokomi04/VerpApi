using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace VErp.Commons.Library
{
    public static class Utils
    {
        public static Guid ToGuid(this string value)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Unicode.GetBytes(value.ToLower()));
            return new Guid(data);
        }
    }
}
