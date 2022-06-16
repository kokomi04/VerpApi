#nullable disable

namespace ActivityLogDB
{
    public partial class UserActivityLogChange
    {
        public long UserActivityLogId { get; set; }
        public string ObjectChange { get; set; }
    }
}
