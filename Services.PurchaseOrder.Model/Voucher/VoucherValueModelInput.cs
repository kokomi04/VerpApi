using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherValueInputModel
    {
        public VoucherValueInputModel()
        {
        }
        public ICollection<VoucherValueRowInputModel> Rows { get; set; }
    }

    public class VoucherValueRowInputModel
    {
        public int VoucherAreaId { get; set; }
        //public bool IsMultiRow { get; set; }
        public long? VoucherValueRowId { get; set; }
        public ICollection<VoucherValueModel> Values { get; set; }
    }


    public class VoucherValueModel 
    {
        public int VoucherAreaFieldId { get; set; }
        public string Value { get; set; }
    }
}
