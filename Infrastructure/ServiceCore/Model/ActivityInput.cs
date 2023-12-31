﻿using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Infrastructure.ServiceCore.Model
{
    public class ActivityInput
    {
        public int UserId { get; set; }
        public EnumActionType ActionId { get; set; }
        public int? BillTypeId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public EnumMessageType MessageTypeId { get; set; }
        public long ObjectId { get; set; }
        public string Message { get; set; }
        public string MessageResourceName { get; set; }
        public string MessageResourceFormatData { get; set; }
        public string Data { get; set; }
        public int SubsidiaryId { get; set; }
        public string IpAddress { get; set; }
    }
}
