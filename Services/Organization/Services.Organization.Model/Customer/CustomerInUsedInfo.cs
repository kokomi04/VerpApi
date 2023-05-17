﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerInUsedInfo
    {
        public int CustomerId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int BillTypeId { get; set; }
        public long BillId { get; set; }
        public string BillCode { get; set; }
        public string Description { get; set; }
    }
}
