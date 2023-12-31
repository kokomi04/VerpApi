﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace VErp.WebApis.VErpApi.Validator
{
    static class Certificate
    {
        public static X509Certificate2 Get(string filePath, string password)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new Exception($"File '{filePath}' not found!");
            }

            try
            {
                return new X509Certificate2(filePath, password);
            }
            catch (Exception ex)
            {

                throw new Exception($"File '{filePath}' exists: {File.Exists(filePath)}\n {ex.Message} \n {ex.StackTrace}");
            }

        }
    }
}