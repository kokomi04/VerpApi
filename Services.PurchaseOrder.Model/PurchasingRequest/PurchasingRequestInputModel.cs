﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingRequest
{
    public class PurchasingRequestInputModel
    {
        //public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public string OrderCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }
        //public bool IsApproved { get; set; }
        //public bool IsDeleted { get; set; }
        //public int? CreatedByUserId { get; set; }
        //public int? UpdatedByUserId { get; set; }
        //public DateTime? CreatedDatetime { get; set; }
        //public DateTime? UpdatedDatetime { get; set; }

        public List<PurchasingRequestDetailInputModel> DetailList { set; get; }
    }
}
