using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductInUsedInfo
    {
        public int ProductId { get; set; }
		public EnumObjectType ObjectTypeId { get; set; }
        public int BillTypeId { get; set; }
        public long BillId { get; set; }
        public string BillCode { get; set; }
        public string Description { get; set; }
}
}
