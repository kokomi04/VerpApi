using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderExcessModel: IMapFrom<OutsourceOrderExcess>
    {
        public long OutsourceOrderExcessId { get; set; }
        public long OutsourceOrderId { get; set; }
        public string Title { get; set; }
        public int UnitId { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public int? DecimalPlace { get; set; }
    }
}
