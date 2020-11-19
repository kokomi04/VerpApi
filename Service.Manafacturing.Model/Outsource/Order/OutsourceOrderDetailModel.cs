using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderDetailModel: IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsoureOrderId { get; set; }
        public long ObjectId { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
    }
}
