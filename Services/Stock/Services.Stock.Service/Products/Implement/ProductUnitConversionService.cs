﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductUnitConversionService : IProductUnitConversionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly MasterDBContext _masterDBContext;

        public ProductUnitConversionService(StockDBContext stockContext, MasterDBContext masterDBContext)
        {
            _stockDbContext = stockContext;
            _masterDBContext = masterDBContext;
        }

        public async Task<PageData<ProductUnitConversionOutput>> GetList(int productId, int page = 0, int size = 0)
        {

            var query = from p in _stockDbContext.ProductUnitConversion
                        select p;

            if (productId > 0)
            {
                query = query.Where(q => q.ProductId == productId);
            }
            var total = query.Count();
            var secondUnitIdList = query.Select(q => q.SecondaryUnitId).ToList();

            var unitList = await _masterDBContext.Unit.AsNoTracking().Where(q => secondUnitIdList.Contains(q.UnitId)).ToListAsync();

            var resultFromDb = new List<ProductUnitConversion>(total);
            if (page > 0 && size > 0)
                resultFromDb = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
            else
                resultFromDb = query.AsNoTracking().ToList();

            var resultList = new List<ProductUnitConversionOutput>(total);
            foreach (var item in resultFromDb)
            {
                var unitObj = unitList.FirstOrDefault(q => q.UnitId == item.SecondaryUnitId);
                var p = new ProductUnitConversionOutput
                {
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    ProductUnitConversionName = item.ProductUnitConversionName,
                    ProductId = item.ProductId,
                    SecondaryUnitId = item.SecondaryUnitId,
                    SecondaryUnitName = unitObj != null ? unitObj.UnitName : null,
                    FactorExpression = item.FactorExpression,
                    ConversionDescription = item.ConversionDescription,
                    IsDefault = item.IsDefault,
                    DecimalPlace = item.DecimalPlace
                };
                resultList.Add(p);
            }
            return (resultList, total);

        }

        public async Task<PageData<ProductUnitConversionByProductOutput>> GetListByProducts(IList<int> productIds, int page = 0, int size = 0)
        {

            if (productIds == null || productIds.Count == 0)
                return (new List<ProductUnitConversionByProductOutput>(), 0);

            var query = from c in _stockDbContext.ProductUnitConversion
                        where productIds.Contains(c.ProductId)
                        select new ProductUnitConversionByProductOutput()
                        {
                            ProductUnitConversionId = c.ProductUnitConversionId,
                            ProductUnitConversionName = c.ProductUnitConversionName,
                            ProductId = c.ProductId,
                            SecondaryUnitId = c.SecondaryUnitId,
                            FactorExpression = c.FactorExpression,
                            ConversionDescription = c.ConversionDescription,
                            IsFreeStyle = c.IsFreeStyle,
                            IsDefault = c.IsDefault,
                            DecimalPlace = c.DecimalPlace
                        };

            var total = query.Count();

            IList<ProductUnitConversionByProductOutput> pagedData;
            if (size <= 0)
            {
                pagedData = await query.ToListAsync();
            }
            else
            {
                pagedData = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            }

            return (pagedData, total);

        }

        public async Task<IList<ProductUnitConversionByProductOutput>> GetByInStockProducts(IList<int> productIds, int stockId, long unixDate)
        {

            if (productIds == null || productIds.Count == 0 || stockId == 0 || unixDate == 0)
                return new List<ProductUnitConversionByProductOutput>();

            var date = unixDate.UnixToDateTime();

            var inStockProducts = (from id in _stockDbContext.InventoryDetail
                                   join i in _stockDbContext.Inventory on id.InventoryId equals i.InventoryId
                                   where i.IsDeleted == false
                                   && id.IsDeleted == false
                                   && i.IsApproved == true
                                   && i.Date <= date
                                   && i.StockId == stockId
                                   && productIds.Contains(id.ProductId)
                                   select new
                                   {
                                       id.ProductId,
                                       id.ProductUnitConversionId
                                   })
                                   .Distinct();

            var result = await (from r in inStockProducts
                                join c in _stockDbContext.ProductUnitConversion on new { r.ProductId, r.ProductUnitConversionId } equals new { c.ProductId, c.ProductUnitConversionId }
                                select new ProductUnitConversionByProductOutput()
                                {
                                    ProductId = c.ProductId,
                                    ProductUnitConversionId = c.ProductUnitConversionId,
                                    ProductUnitConversionName = c.ProductUnitConversionName,
                                    SecondaryUnitId = c.SecondaryUnitId,
                                    FactorExpression = c.FactorExpression,
                                    ConversionDescription = c.ConversionDescription,
                                    IsFreeStyle = c.IsFreeStyle,
                                    IsDefault = c.IsDefault,
                                    DecimalPlace = c.DecimalPlace
                                })
                                .ToListAsync();

            return result;

        }
    }
}

