using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Stock
{
    public class InternalProductProcessStatus
    {
        public long ProductId { get; set; }
        public EnumProductionProcessStatus ProcessStatus { get; set; }
    }
}
