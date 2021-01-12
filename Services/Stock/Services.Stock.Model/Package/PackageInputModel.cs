using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageInputModel
    {
        //public long PackageId { get; set; }
        //public int PackageTypeId { get; set; }

        public string PackageCode { get; set; }
        public int? LocationId { get; set; }

        //public int StockId { set; get; }

        //public int ProductId { set; get; }

        //public string Date { get; set; }
        public long ExpiryTime { get; set; }

        public string Description { get; set; }

        public string OrderCode { get; set; }

        /// <summary>
        /// Purchase order code 
        /// </summary>
        public string POCode { get; set; }

        public string ProductionOrderCode { get; set; }

        //public int PrimaryUnitId { get; set; }
        //public decimal PrimaryQuantity { get; set; }

        //public int ProductUnitConversionId { set; get; }

        //public decimal ProductUnitConversionQuantity { get; set; }


        //public decimal PrimaryQuantityWaiting { get; set; }
        //public decimal PrimaryQuantityRemaining { get; set; }

        //public decimal ProductUnitConversionWaitting { get; set; }
        //public decimal ProductUnitConversionRemaining { get; set; }


        //public virtual Location Location { get; set; }
    }
}
