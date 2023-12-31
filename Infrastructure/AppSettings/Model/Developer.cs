﻿using System;
using System.Linq;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class Developer
    {
        public string[] Users { get; set; }
        public string[] SubsidiaryCodes { get; set; }

        public bool IsDeveloper(string userName, string subsidiaryCode)
        {
            return Users?.Contains(userName) == true && SubsidiaryCodes?.Contains(subsidiaryCode) == true;
        }
    }
}
