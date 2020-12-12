using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Constants
{
    public static class StringTemplateConstants
    {
        public static readonly string CODE = $"%CODE%";
        public static readonly string FID = $"%{AccountantConstants.F_IDENTITY}%";
        public static readonly string SNUMBER = $"%S_NUMBER%";
    }
}
