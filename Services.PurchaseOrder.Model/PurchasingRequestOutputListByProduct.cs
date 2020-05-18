﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestOutputListByProduct: PurchasingRequestOutputList
    {
        public long PurchasingRequestDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public string Description { get; set; }
    }
}
