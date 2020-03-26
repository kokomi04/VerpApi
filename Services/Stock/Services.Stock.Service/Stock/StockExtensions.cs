using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Stock
{
    public static class StockExtensions
    {
        public static void AddRemaining(this Package package, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            package.PrimaryQuantityRemaining = package.PrimaryQuantityRemaining.AddDecimal(primaryQuantity);
            package.ProductUnitConversionRemaining = package.ProductUnitConversionRemaining.AddDecimal(productUnitConversionQuantity);
        }

        public static void AddWaiting(this Package package, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            package.PrimaryQuantityWaiting = package.PrimaryQuantityWaiting.AddDecimal(primaryQuantity);
            package.ProductUnitConversionWaitting = package.ProductUnitConversionWaitting.AddDecimal(productUnitConversionQuantity);
        }


        public static void AddRemaining(this StockProduct stockProduct, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            stockProduct.PrimaryQuantityRemaining = stockProduct.PrimaryQuantityRemaining.AddDecimal(primaryQuantity);
            stockProduct.ProductUnitConversionRemaining = stockProduct.ProductUnitConversionRemaining.AddDecimal(productUnitConversionQuantity);
        }

        public static void AddWaiting(this StockProduct stockProduct, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            stockProduct.PrimaryQuantityWaiting = stockProduct.PrimaryQuantityWaiting.AddDecimal(primaryQuantity);
            stockProduct.ProductUnitConversionWaitting = stockProduct.ProductUnitConversionWaitting.AddDecimal(productUnitConversionQuantity);
        }

        public static void AddQuantity(this InventoryDetail detail, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            detail.PrimaryQuantity = detail.PrimaryQuantity.AddDecimal(primaryQuantity);
            detail.ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity.AddDecimal(productUnitConversionQuantity);
        }

        public static void AddTransfer(this PackageRef packageRef, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            packageRef.PrimaryQuantity = packageRef.PrimaryQuantity?.AddDecimal(primaryQuantity);
            packageRef.ProductUnitConversionQuantity = packageRef.ProductUnitConversionQuantity?.AddDecimal(productUnitConversionQuantity);
        }
    }
}
