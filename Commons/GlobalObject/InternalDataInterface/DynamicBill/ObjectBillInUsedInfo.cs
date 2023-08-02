using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
{
    public class ObjectBillInUsedInfo
    {
        public long Id { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int BillTypeId { get; set; }
        public long BillId { get; set; }
        public string BillCode { get; set; }
        public string Description { get; set; }
    }
}
