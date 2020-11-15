using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderPartDetailInfo: OutsourceOrderDetailModel
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public int Quanity { get; set; }
    }
}
