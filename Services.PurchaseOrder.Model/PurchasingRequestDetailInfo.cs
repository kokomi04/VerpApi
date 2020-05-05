﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestDetailInfo
    {
        public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }

        public long PurchasingRequestDetailId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        
    }
}
