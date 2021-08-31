﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class RefEmployee
    {
        public int UserId { get; set; }
        public int SubsidiaryId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int? GenderId { get; set; }
        public bool IsDeleted { get; set; }
        public long? AvatarFileId { get; set; }
        public int EmployeeTypeId { get; set; }
        public int UserStatusId { get; set; }
    }
}
