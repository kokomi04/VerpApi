using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.Activity
{
    public class UserActivityLogOuputModel
    {
        public int UserId { set; get; }

        public string UserName { set; get; }

        public long ObjectId { set; get; }

        //public int ObjectTypeId { set; get; }

        //public string ObjectName { set; get; }

        public int ActionId { set; get; }

        public string ActionName { set; get; }

        //public int MessageTypeId { set; get; }

        //public string MessageTypeName { set; get; }

        public string Message { set; get; }

        public long CreatedDatetimeUtc { set; get; }

    }


}
