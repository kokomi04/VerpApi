﻿using System;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockProductPackageDetail
    {
        public int ProductId { get; set; }
        public long PackageId { get; set; }
        public string PackageCode { get; set; }

        public int StockId { get; set; }
        public string StockName { get; set; }

        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public long? Date { get; set; }
        public long? ExpriredDate { get; set; }
        public string Description { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int DefaultDecimalPlace { get; set; }
        public int DefaultUnitConversionId { get; set; }
        public int DefaultUnitConversionName { get; set; }



        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public int DecimalPlace { get; set; }

        //public int? SecondaryUnitId { get; set; }
        public decimal ProductUnitConversionQualtity { get; set; }
        public decimal SecondaryUnitQualtity { get { return ProductUnitConversionQualtity; } }
        public EnumPackageType PackageTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }

        public string OrderCode { get; set; }

        /// <summary>
        /// Purchase order code 
        /// </summary>
        public string POCode { get; set; }

        public string ProductionOrderCode { get; set; }

     
       
    }
}
