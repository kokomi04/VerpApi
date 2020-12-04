using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderDetailModel: IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsourceOrderId { get; set; }
        [Required]
        public long ObjectId { get; set; }
        [Required]
        [Range(0.00001, double.MaxValue)]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public decimal Tax { get; set; }
    }

    public class OutsourceOrderDetailInfo: OutsourceOrderDetailModel
    {
        public string ProductPartCode { get; set; }
        public string ProductPartName { get; set; }
        public string UnitName { get; set; }
        public decimal QuantityOrigin { get; set; }
        public decimal QuantityProcessed{ get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long OutsourcePartRequestFinishDate { get; set; }
    }
}
