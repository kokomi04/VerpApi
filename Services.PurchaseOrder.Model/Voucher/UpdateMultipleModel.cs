﻿namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class UpdateMultipleModel
    {
        public string FieldName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public long[] FIds { get; set; }
    }
}