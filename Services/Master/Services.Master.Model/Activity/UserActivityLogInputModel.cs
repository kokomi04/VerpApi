using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.Activity
{
    public class UserActivityLogInputModel
    {
        public int UserId { set; get; }

        public long ObjectId { set; get; }

        public int ObjectTypeId { set; get; }

        public int ActionId { set; get; }

        public int MessageTypeId { set; get; }

        public string Message { set; get; }
    }
}
