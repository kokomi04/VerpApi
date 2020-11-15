using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderDetailModel: IMapFrom<OutsourceOrderDetail>
    {
        public int OutsourceOrderDetailId { get; set; }
        public int OutsoureOrderId { get; set; }
        public int ObjectId { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
    }
}
