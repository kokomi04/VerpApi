using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.Users
{
    public class UserBasicInfoOutput
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public long? AvatarFileId { get; set; }
    }
}
