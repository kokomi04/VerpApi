using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Activity
{
    public class UserActivityLogOuputModel
    {
        public int UserId { set; get; }

        public string UserName { set; get; }
        public string FullName { get; set; }
        public long? AvatarFileId { get; set; }
        public EnumAction? ActionId { set; get; }

        public EnumMessageType MessageTypeId { set; get; }

        public string Message { set; get; }

        public long CreatedDatetimeUtc { set; get; }

    }


}
