using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Activity
{
    public class UserActivityLogOuputModel
    {
        public int UserId { set; get; }
        public int SubsidiaryId { set; get; }
        public string UserName { set; get; }
        public string FullName { get; set; }
        public long? AvatarFileId { get; set; }
        public EnumActionType? ActionId { set; get; }

        public EnumMessageType MessageTypeId { set; get; }

        public string Message { set; get; }
        //public string MessageResourceName { set; get; }
        //public string MessageResourceFormatData { get; set; }

        public long CreatedDatetimeUtc { set; get; }
        public string IpAddress { get; set; }
        public long UserActivityLogId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public int? BillTypeId { get; set; }
    }


}
