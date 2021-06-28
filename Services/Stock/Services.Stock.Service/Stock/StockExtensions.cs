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
            if (package.PrimaryQuantityRemaining == 0)
            {
                package.ProductUnitConversionRemaining = 0;
            }

            if (package.ProductUnitConversionRemaining == 0)
            {
                package.PrimaryQuantityRemaining = 0;
            }
        }

        public static void AddWaiting(this Package package, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            package.PrimaryQuantityWaiting = package.PrimaryQuantityWaiting.AddDecimal(primaryQuantity);
            package.ProductUnitConversionWaitting = package.ProductUnitConversionWaitting.AddDecimal(productUnitConversionQuantity);

            if (package.PrimaryQuantityWaiting == 0)
            {
                package.ProductUnitConversionWaitting = 0;
            }

            if (package.ProductUnitConversionWaitting == 0)
            {
                package.PrimaryQuantityWaiting = 0;
            }
        }


        public static void AddRemaining(this StockProduct stockProduct, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            stockProduct.PrimaryQuantityRemaining = stockProduct.PrimaryQuantityRemaining.AddDecimal(primaryQuantity);
            stockProduct.ProductUnitConversionRemaining = stockProduct.ProductUnitConversionRemaining.AddDecimal(productUnitConversionQuantity);
            if (stockProduct.PrimaryQuantityRemaining == 0)
            {
                stockProduct.ProductUnitConversionRemaining = 0;
            }

            if (stockProduct.ProductUnitConversionRemaining == 0)
            {
                stockProduct.PrimaryQuantityRemaining = 0;
            }
        }

        public static void AddWaiting(this StockProduct stockProduct, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            stockProduct.PrimaryQuantityWaiting = stockProduct.PrimaryQuantityWaiting.AddDecimal(primaryQuantity);
            stockProduct.ProductUnitConversionWaitting = stockProduct.ProductUnitConversionWaitting.AddDecimal(productUnitConversionQuantity);
            if (stockProduct.PrimaryQuantityWaiting == 0)
            {
                stockProduct.ProductUnitConversionWaitting = 0;
            }

            if (stockProduct.ProductUnitConversionWaitting == 0)
            {
                stockProduct.PrimaryQuantityWaiting = 0;
            }
        }

        public static void AddQuantity(this InventoryDetail detail, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            detail.PrimaryQuantity = detail.PrimaryQuantity.AddDecimal(primaryQuantity);
            detail.ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity.AddDecimal(productUnitConversionQuantity);
            if (detail.PrimaryQuantity == 0)
            {
                detail.ProductUnitConversionQuantity = 0;
            }

            if (detail.ProductUnitConversionQuantity == 0)
            {
                detail.PrimaryQuantity = 0;
            }
        }

        public static void AddTransfer(this PackageRef packageRef, decimal primaryQuantity, decimal productUnitConversionQuantity)
        {
            packageRef.PrimaryQuantity = packageRef.PrimaryQuantity?.AddDecimal(primaryQuantity);
            packageRef.ProductUnitConversionQuantity = packageRef.ProductUnitConversionQuantity?.AddDecimal(productUnitConversionQuantity);

            if (packageRef.PrimaryQuantity == 0)
            {
                packageRef.ProductUnitConversionQuantity = 0;
            }

            if (packageRef.ProductUnitConversionQuantity == 0)
            {
                packageRef.PrimaryQuantity = 0;
            }
        }
    }
}
