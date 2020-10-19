using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class Developer
    {
        public string[] Users { get; set; }
        public int[] SubsidiaryIds { get; set; }

        public bool IsDeveloper(string userName, int subsidiaryId)
        {
            return Users?.Contains(userName) == true && SubsidiaryIds?.Contains(subsidiaryId) == true;
        }
    }
}
