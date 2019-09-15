using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public Guid UserNameHash { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsDeleted { get; set; }
        public int UserStatusId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int? RoleId { get; set; }
    }
}
