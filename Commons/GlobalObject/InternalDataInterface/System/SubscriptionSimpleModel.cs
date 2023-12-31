namespace VErp.Commons.GlobalObject.InternalDataInterface.System
{
    public class SubscriptionSimpleModel : CheckSubscriptionSimpleModel
    {
        public int UserId { get; set; }
    }

    public class CheckSubscriptionSimpleModel
    {
        public int ObjectTypeId { get; set; }
        public int? BillTypeId { get; set; }
        public long ObjectId { get; set; }
    }

    public class SubscriptionToThePermissionPersonSimpleModel
    {
        public int ObjectTypeId { get; set; }
        public int? BillTypeId { get; set; }
        public int ModuleId { get; set; }
        public int PermissionId { get; set; }
        public long ObjectId { get; set; }
    }
}