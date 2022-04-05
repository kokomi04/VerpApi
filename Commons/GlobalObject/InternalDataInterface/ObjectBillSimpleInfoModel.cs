using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class ObjectBillSimpleInfoModel
    {
        public int ObjectTypeId { get; set; }
        public long ObjectBill_F_Id { get; set; }
        public string ObjectBillCode { get; set; }
    }
}
