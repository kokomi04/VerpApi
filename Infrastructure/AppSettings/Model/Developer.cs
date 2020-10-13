using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class Developer
    {
        public string[] Users { get; set; }
        public string[] Roles { get; set; }

        public bool IsDeveloper(string userName, string roleName)
        {
            return Users.Contains(userName) && Roles.Contains(roleName);
        }
    }
}
