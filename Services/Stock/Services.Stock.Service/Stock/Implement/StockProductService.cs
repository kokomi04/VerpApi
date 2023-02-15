﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Excel;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Products;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class StockProductService : IStockProductService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly StockDBContext _stockContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductService productService;
        private readonly IProductUnitConversionService puService;
        private readonly IPackageCustomPropertyService packageCustomPropertyService;

        public StockProductService(
            MasterDBContext masterDBContext,
            OrganizationDBContext organizationDBContext,
            StockDBContext stockContext,
            ICurrentContextService currentContextService,
            IProductService productService,
            IPackageCustomPropertyService packageCustomPropertyService,
            IProductUnitConversionService puService)
        {
            _organizationDBContext = organizationDBContext;
            _masterDBContext = masterDBContext;
            _stockContext = stockContext;
            _currentContextService = currentContextService;
            this.productService = productService;
            this.packageCustomPropertyService = packageCustomPropertyService;
            this.puService = puService;
        }

        public async Task<PageData<StockOutput>> StockGetListByPermission(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = from p in _stockContext.Stock
                        select p;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.StockName.Contains(keyword)
                        select q;
            }
            query = query.InternalFilter(filters);
            var total = await query.CountAsync();
            var lstData = size > 0 ? await query.Skip((page - 1) * size).Take(size).ToListAsync() : await query.ToListAsync();

            var pagedData = new List<StockOutput>();
            foreach (var item in lstData)
            {
                var stockInfo = new StockOutput()
                {
                    StockId = item.StockId,
                    StockName = item.StockName,
                    StockCode = item.StockCode,
                    Description = item.Description,
                    StockKeeperId = item.StockKeeperId,
                    StockKeeperName = item.StockKeeperName,
                    Type = item.Type,
                    Status = item.Status
                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }



        public async Task<IList<StockWarning>> StockWarnings()
        {
            var lstMinMaxWarnings = await (
                 from sp in _stockContext.StockProduct
                 join p in _stockContext.Product on sp.ProductId equals p.ProductId
                 join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                 where sp.PrimaryQuantityRemaining <= ps.AmountWarningMin
                 || sp.PrimaryQuantityRemaining >= ps.AmountWarningMax
                 select new
                 {
                     sp.StockId,
                     p.ProductId,
                     p.ProductCode,
                     p.ProductName,
                     StockWarningTypeId = sp.PrimaryQuantityRemaining <= ps.AmountWarningMin ? EnumWarningType.Min : EnumWarningType.Max,
                 })
                 .ToListAsync();

            var lstExpiredWarnings = await (
                from pk in _stockContext.Package
                    //join d in _stockContext.InventoryDetail on pk.InventoryDetailId equals d.InventoryDetailId
                    //join iv in _stockContext.Inventory on d.InventoryId equals iv.InventoryId
                join p in _stockContext.Product on pk.ProductId equals p.ProductId
                join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                where pk.ExpiryTime < DateTime.UtcNow
                select new
                {
                    pk.StockId,
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    pk.PackageCode,
                    StockWarningTypeId = EnumWarningType.Expired
                })
                .ToListAsync();

            var result = new List<StockWarning>();
            var stocks = await _stockContext.Stock.ToListAsync();
            foreach (var s in stocks)
            {
                var warnings = lstMinMaxWarnings
                    .Where(w => w.StockId == s.StockId)
                    .Select(w => new StockWarningDetail
                    {
                        ProductId = w.ProductId,
                        ProductCode = w.ProductCode,
                        ProductName = w.ProductName,
                        StockWarningTypeId = w.StockWarningTypeId,
                        PackageCode = null
                    });

                warnings.Union(
                    lstExpiredWarnings.Where(w => w.StockId == s.StockId)
                    .Select(w => new StockWarningDetail
                    {
                        ProductId = w.ProductId,
                        ProductCode = w.ProductCode,
                        ProductName = w.ProductName,
                        StockWarningTypeId = w.StockWarningTypeId,
                        PackageCode = null
                    })
                    );

                result.Add(new StockWarning()
                {
                    StockId = s.StockId,
                    StockName = s.StockName,
                    Warnings = warnings.OrderBy(p => p.ProductCode).ToList()
                });
            }
            return result;
        }

        public async Task<PageData<StockProductListOutput>> StockProducts(int stockId, string keyword, IList<int> productTypeIds, IList<int> productCateIds, IList<EnumWarningType> stockWarningTypeIds, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var productQuery = (
                 from p in _stockContext.Product
                 select new
                 {
                     p.ProductId,
                     p.UnitId,
                     p.ProductCode,
                     p.ProductName,
                     p.ProductTypeId,
                     p.ProductCateId
                 }
                );
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }

            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var types = productTypeIds.Select(t => (int?)t);
                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                productQuery = from p in productQuery
                               where productCateIds.Contains(p.ProductCateId)
                               select p;
            }

            var productRemaining = from sp in _stockContext.StockProduct
                                   join p in productQuery on sp.ProductId equals p.ProductId
                                   where sp.StockId == stockId
                                   group sp by new { sp.ProductId, sp.StockId } into g
                                   //from p in g
                                   select new
                                   {
                                       g.Key.ProductId,
                                       PrimaryQuantityRemaining = g.Sum(q => q.PrimaryQuantityRemaining)
                                   };


            var query = from sp in productRemaining
                        join p in productQuery on sp.ProductId equals p.ProductId
                        join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                        where sp.PrimaryQuantityRemaining > 0
                        select new
                        {
                            p.ProductId,
                            sp.PrimaryQuantityRemaining,
                            ps.AmountWarningMin,
                            ps.AmountWarningMax,
                        };

            if (stockWarningTypeIds != null && stockWarningTypeIds.Count > 0)
            {
                if (stockWarningTypeIds.Contains(EnumWarningType.Min))
                {
                    query = from p in query
                            where p.PrimaryQuantityRemaining <= p.AmountWarningMin
                            select p;
                }

                if (stockWarningTypeIds.Contains(EnumWarningType.Max))
                {
                    query = from p in query
                            where p.AmountWarningMax >= p.AmountWarningMin
                            select p;
                }


                if (stockWarningTypeIds.Contains(EnumWarningType.Expired))
                {
                    var productWithExprires = from pk in _stockContext.Package
                                                  //from iv in _stockContext.Inventory
                                                  //join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                                                  //join pk in _stockContext.Package on d.InventoryDetailId equals pk.InventoryDetailId
                                              where pk.StockId == stockId && pk.ExpiryTime < DateTime.UtcNow
                                              select new
                                              {
                                                  pk.ProductId,
                                              };

                    query = from p in query
                            join e in productWithExprires on p.ProductId equals e.ProductId
                            select p;
                }
            }

            var queryData = from q in query
                            join sp in _stockContext.StockProduct on q.ProductId equals sp.ProductId
                            join c in _stockContext.ProductUnitConversion on sp.ProductUnitConversionId equals c.ProductUnitConversionId into cs
                            from c in cs.DefaultIfEmpty()
                            select new
                            {
                                q.ProductId,
                                sp.PrimaryQuantityRemaining,
                                sp.ProductUnitConversionId,
                                c.ProductUnitConversionName,
                                sp.ProductUnitConversionRemaining,
                                c.DecimalPlace
                            };

            var total = await queryData.CountAsync();
            var lstData = size > 0 ? await queryData.Skip((page - 1) * size).Take(size).ToListAsync() : await queryData.ToListAsync();
            var productIds = lstData.Select(p => p.ProductId).ToList();

            if (productIds.Count == 0)
            {
                return (new List<StockProductListOutput>(), total);
            }
            var extraInfos = await (
                from p in _stockContext.Product
                join ex in _stockContext.ProductExtraInfo on p.ProductId equals ex.ProductId into exs
                from ex in exs.DefaultIfEmpty()
                where productIds.Contains(p.ProductId)
                select new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductTypeId,
                    p.ProductCateId,
                    p.UnitId,
                    ex.Specification
                }
                )
                .ToListAsync();

            var pagedData = new List<StockProductListOutput>();

            foreach (var item in lstData)
            {
                var extra = extraInfos.FirstOrDefault(p => p.ProductId == item.ProductId);

                var stockInfo = new StockProductListOutput()
                {
                    ProductId = item.ProductId,
                    ProductCode = extra?.ProductCode,
                    ProductName = extra?.ProductName,
                    ProductTypeId = extra?.ProductTypeId,
                    ProductCateId = extra?.ProductCateId ?? 0,
                    Specification = extra?.Specification,
                    UnitId = extra?.UnitId ?? 0,
                    PrimaryQuantityRemaining = item.PrimaryQuantityRemaining,

                    ProductUnitConversionId = item.ProductUnitConversionId ?? 0,
                    ProductUnitConversionName = item.ProductUnitConversionName,
                    ProductUnitConversionRemaining = item.ProductUnitConversionRemaining,
                    DecimalPlace = item.DecimalPlace
                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }

        public async Task<Dictionary<int, RemainStock[]>> GetRemainStockByProducts(int[] productIds)
        {

            var remainStocks = (
                await _stockContext.StockProduct.Where(sp => productIds.Contains(sp.ProductId))
                    .GroupBy(sp => new { sp.ProductId, sp.StockId })
                    .Select(g => new
                    {
                        g.Key.ProductId,
                        g.Key.StockId,
                        PrimaryQuantityRemaining = g.Sum(q => q.PrimaryQuantityRemaining)
                    })
                    .Where(s => s.PrimaryQuantityRemaining > 0)
                    .Join(_stockContext.Stock, rs => rs.StockId, s => s.StockId, (rs, s) => new
                    {
                        rs.StockId,
                        rs.ProductId,
                        rs.PrimaryQuantityRemaining,
                        s.StockName
                    })
                    .ToListAsync()
                )
                .GroupBy(s => s.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(s => new RemainStock
                {
                    StockId = s.StockId,
                    PrimaryQuantityRemaining = s.PrimaryQuantityRemaining,
                    StockName = s.StockName
                }).ToArray());
            return remainStocks;
        }

        public async Task<PageData<StockProductPackageDetail>> StockProductPackageDetails(IList<int> stockIds, int productId, int page, int size)
        {
            var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
            var packages = _stockContext.Package.AsQueryable();
            if (stockIds?.Count > 0)
            {
                packages = packages.Where(p => stockIds.Contains(p.StockId));
            }
            var query = (
                from pk in packages
                join st in _stockContext.Stock on pk.StockId equals st.StockId
                join p in _stockContext.Product on pk.ProductId equals p.ProductId
                //join c in _stockContext.ProductUnitConversion on pk.ProductUnitConversionId equals c.ProductUnitConversionId into cs
                //from c in cs.DefaultIfEmpty()
                join l in _stockContext.Location on pk.LocationId equals l.LocationId into ls
                from l in ls.DefaultIfEmpty()
                where pk.ProductId == productId && pk.PrimaryQuantityRemaining > 0
                select new
                {
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    pk.StockId,
                    st.StockName,
                    pk.OrderCode,
                    pk.ProductionOrderCode,
                    pk.Pocode,
                    pk.ProductId,
                    LocationId = pk.LocationId,
                    LocationName = l == null ? null : l.Name,
                    Date = pk.Date,
                    ExpriredDate = pk.ExpiryTime,
                    pk.Description,
                    PrimaryUnitId = p.UnitId,
                    PrimaryQuantity = pk.PrimaryQuantityRemaining,
                    //SecondaryUnitId = c == null ? (int?)null : c.SecondaryUnitId,
                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    //ProductUnitConversionName = c == null ? null : c.ProductUnitConversionName,
                    ProductUnitConversionQualtity = pk.ProductUnitConversionRemaining,
                    PackageTypeId = (EnumPackageType)pk.PackageTypeId,
                    RefObjectCode = "",
                    pk.CustomPropertyValue
                    //c.DecimalPlace
                }
                );
            var total = await query.CountAsync();
            switch ((EnumStockOutputRule?)productStockInfo.StockOutputRuleId)
            {
                case EnumStockOutputRule.None:
                case EnumStockOutputRule.Fifo:
                    query = from pk in query
                            orderby pk.Date
                            select pk;
                    break;
                case EnumStockOutputRule.Lifo:
                    query = from pk in query
                            orderby pk.Date descending
                            select pk;
                    break;
            }

            var lstData = size > 0 ? await query.Skip((page - 1) * size).Take(size).ToListAsync() : await query.ToListAsync();
            var productIds = lstData.Select(d => d.ProductId).ToList();
            var pus = await _stockContext.ProductUnitConversion.Where(pu => productIds.Contains(pu.ProductId)).ToListAsync();

            var data = lstData.Select(pk =>
            {
                var pu = pus.FirstOrDefault(u => u.ProductId == pk.ProductId && u.ProductUnitConversionId == pk.ProductUnitConversionId);
                var puDefault = pus.FirstOrDefault(u => u.ProductId == pk.ProductId && u.IsDefault);
                return new StockProductPackageDetail()
                {
                    ProductId = pk.ProductId,
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    StockId = pk.StockId,
                    StockName = pk.StockName,
                    LocationId = pk.LocationId,
                    LocationName = pk.LocationName,
                    Date = pk.Date.HasValue ? pk.Date.Value.GetUnix() : (long?)null,
                    ExpriredDate = pk.ExpriredDate.HasValue ? pk.ExpriredDate.Value.GetUnix() : (long?)null,
                    Description = pk.Description,

                    PrimaryUnitId = pk.PrimaryUnitId,
                    PrimaryQuantity = pk.PrimaryQuantity,
                    DefaultUnitConversionId = puDefault.ProductUnitConversionId,
                    DefaultUnitConversionName = puDefault.ProductUnitConversionId,
                    DefaultDecimalPlace = puDefault.DecimalPlace,

                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    ProductUnitConversionName = pu.ProductUnitConversionName,
                    DecimalPlace = pu.DecimalPlace,

                    ProductUnitConversionQualtity = pk.ProductUnitConversionQualtity,

                    PackageTypeId = pk.PackageTypeId,
                    RefObjectId = null,
                    RefObjectCode = "",
                    OrderCode = pk.OrderCode,
                    POCode = pk.Pocode,
                    ProductionOrderCode = pk.ProductionOrderCode,
                    CustomPropertyValue = pk.CustomPropertyValue?.JsonDeserialize<Dictionary<int, object>>()
                };
            })
            .ToList();

            return (data, total);
        }


        public async Task<(Stream stream, string fileName, string contentType)> StockProductPackageDetailsExport(IList<int> stockIds, int productId, int page, int size)
        {
            var data = await StockProductPackageDetails(stockIds, productId, 1, 0);


            var columns = new List<ExelExportColumn>()
            {
                new ExelExportColumn
                {
                    Title="Kho",
                    FieldName = nameof(StockProductPackageDetail.StockName)
                },
                 new ExelExportColumn
                {
                    Title="Vị trí",
                    FieldName = nameof(StockProductPackageDetail.LocationName)
                },

                new ExelExportColumn
                {
                    Title="Mã kiện",
                    FieldName = nameof(StockProductPackageDetail.PackageCode)
                },
                new ExelExportColumn
                {
                    Title="Mô tả",
                    FieldName = nameof(StockProductPackageDetail.Description)
                },
                 new ExelExportColumn
                {
                    Title="ĐVCĐ",
                    FieldName = nameof(StockProductPackageDetail.ProductUnitConversionName)
                },
                  new ExelExportColumn
                {
                    Title="Số lượng ĐVC",
                    FieldName = nameof(StockProductPackageDetail.PrimaryQuantity),
                    DataTypeId = EnumDataType.Decimal,
                    DecimalPlace = data.List.FirstOrDefault()?.DefaultDecimalPlace
                }, new ExelExportColumn
                {
                    Title="Số lượng ĐVCĐ",
                    FieldName = nameof(StockProductPackageDetail.ProductUnitConversionQualtity),
                    DataTypeId = EnumDataType.Decimal,
                    DecimalPlace = data.List.FirstOrDefault()?.DecimalPlace
                },
                  new ExelExportColumn
                {
                    Title="Ngày nhập",
                    FieldName = nameof(StockProductPackageDetail.Date),
                    DataTypeId = EnumDataType.Date,
                },
                  new ExelExportColumn
                {
                    Title="Hạn sử dụng",
                    FieldName = nameof(StockProductPackageDetail.ExpriredDate),
                    DataTypeId = EnumDataType.Date,
                },
                   new ExelExportColumn
                {
                    Title="Mã mua hàng",
                    FieldName = nameof(StockProductPackageDetail.POCode),
                },
                   new ExelExportColumn
                {
                    Title="Mã lsx",
                    FieldName = nameof(StockProductPackageDetail.ProductionOrderCode),
                },
                   new ExelExportColumn
                {
                    Title="Mã đơn hàng",
                    FieldName = nameof(StockProductPackageDetail.OrderCode),
                }
            };

            var packageProps = await packageCustomPropertyService.Get();

            foreach (var p in packageProps)
            {
                columns.Add(new ExelExportColumn
                {
                    Title = p.Title,
                    FieldName = nameof(StockProductPackageDetail.CustomPropertyValue) + p.PackageCustomPropertyId,
                });
            }

            var productInfo = await productService.ProductInfo(productId);

            var pus = await puService.GetListByProducts(new[] { productId });

            var headerData = new NonCamelCaseDictionary
            {
                {"Mã mặt hàng", productInfo.ProductCode },
                {"Tên mặt hàng", productInfo.ProductName },
                {"Quy cách", productInfo.Extra?.Specification },
                {"Đơn vị tính", pus?.List?.FirstOrDefault(u=>u.IsDefault)?.ProductUnitConversionName },
            };

            var excelData = data.List.ToNonCamelCaseDictionaryList((model, row, result) =>
            {
                foreach (var p in packageProps)
                {
                    var value = model.CustomPropertyValue?.ContainsKey(p.PackageCustomPropertyId.Value) == true ? model.CustomPropertyValue[p.PackageCustomPropertyId.Value] : null;

                    row.Add(nameof(StockProductPackageDetail.CustomPropertyValue) + p.PackageCustomPropertyId, value);
                }

            });

            var util = new ExcelExportUtils(productInfo.ProductCode + " Danh sách kiện của mặt hàng", _currentContextService, excelData, columns, headerData, 2);
            return await util.WriteExcel();
        }


        public async Task<PageData<LocationProductPackageOuput>> LocationProductPackageDetails(int stockId, int? locationId, IList<int> productTypeIds, IList<int> productCateIds, int page, int size)
        {
            var products = _stockContext.Product.AsQueryable();
            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var typeIds = productTypeIds.Cast<int?>();
                products = from p in products
                           where typeIds.Contains(p.ProductTypeId)
                           select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                products = from p in products
                           where productCateIds.Contains(p.ProductCateId)
                           select p;
            }
            var query = (
                from pk in _stockContext.Package
                join p in products on pk.ProductId equals p.ProductId
                join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                join c in _stockContext.ProductUnitConversion on pk.ProductUnitConversionId equals c.ProductUnitConversionId into cs
                from c in cs.DefaultIfEmpty()
                where pk.StockId == stockId && pk.LocationId == locationId && pk.PrimaryQuantityRemaining > 0
                orderby pk.Date descending
                select new
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    Date = pk.Date,
                    ExpriredDate = pk.ExpiryTime,
                    pk.Description,
                    PrimaryUnitId = p.UnitId,
                    PrimaryQuantity = pk.PrimaryQuantityRemaining,
                    SecondaryUnitId = c == null ? (int?)null : c.SecondaryUnitId,
                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    ProductUnitConversionName = c == null ? null : c.ProductUnitConversionName,
                    ProductUnitConversionQualtity = pk.ProductUnitConversionRemaining,
                    PackageTypeId = (EnumPackageType)pk.PackageTypeId,
                    RefObjectCode = "",
                    c.DecimalPlace
                }
                );
            var total = await query.CountAsync();

            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var data = lstData.Select(pk => new LocationProductPackageOuput()
            {
                ProductId = pk.ProductId,
                ProductCode = pk.ProductCode,
                ProductName = pk.ProductName,
                PackageId = pk.PackageId,
                PackageCode = pk.PackageCode,
                Date = pk.Date.HasValue ? pk.Date.Value.GetUnix() : (long?)0,
                ExpriredDate = pk.ExpriredDate.HasValue ? pk.ExpriredDate.Value.GetUnix() : (long?)null,
                Description = pk.Description,
                PrimaryUnitId = pk.PrimaryUnitId,
                PrimaryQuantity = pk.PrimaryQuantity,
                SecondaryUnitId = pk.SecondaryUnitId,
                ProductUnitConversionId = pk.ProductUnitConversionId,
                ProductUnitConversionName = pk.ProductUnitConversionName,
                ProductUnitConversionQualtity = pk.ProductUnitConversionQualtity,
                RefObjectId = null,
                PackageTypeId = pk.PackageTypeId,
                RefObjectCode = "",
                DecimalPlace = pk.DecimalPlace

            }).ToList();

            return (data, total);
        }


        public async Task<PageData<StockProductQuantityWarning>> GetStockProductQuantityWarning(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, IList<int> rangeQuantityRemaining, bool? isMinOrMax, string sortBy, bool asc, int page, int size, Clause filters)
        {
            keyword = (keyword ?? "").Trim();

            var productQuery = _stockContext.Product.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }

            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var productTypes = await _stockContext.ProductType.ToListAsync();

                var types = new List<int?>();
                foreach (var productTypeId in productTypeIds)
                {
                    var st = new Stack<int>();
                    st.Push(productTypeId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        types.Add(parentId);
                        var children = productTypes.Where(p => p.ParentProductTypeId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductTypeId);
                        }
                    }
                }
                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                var productCates = await _stockContext.ProductCate.ToListAsync();

                var cates = new List<int>();
                foreach (var productCateId in productCateIds)
                {
                    var st = new Stack<int>();
                    st.Push(productCateId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        cates.Add(parentId);
                        var children = productCates.Where(p => p.ParentProductCateId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductCateId);
                        }
                    }
                }
                productQuery = from p in productQuery
                               where cates.Contains(p.ProductCateId)
                               select p;
            }

            var stockProductAsQueryable = _stockContext.StockProduct.AsQueryable();

            if (stockIds != null && stockIds.Count > 0)
                stockProductAsQueryable = from sp in stockProductAsQueryable where stockIds.Contains(sp.StockId) select sp;

            var stockProductDataAsQueryAble = from sp in stockProductAsQueryable
                                              join s in _stockContext.Stock on sp.StockId equals s.StockId
                                              select new
                                              {
                                                  s.StockName,
                                                  sp.StockId,
                                                  sp.ProductId,
                                                  sp.PrimaryQuantityRemaining,
                                                  sp.ProductUnitConversionId,
                                                  sp.ProductUnitConversionRemaining,
                                              };

            var StockRemaningAsQueryable = from spd in stockProductDataAsQueryAble
                                           group spd by spd.ProductId into g
                                           select new
                                           {
                                               ProductId = g.Key,
                                               TotalQuantityRemaning = g.Sum(x => x.PrimaryQuantityRemaining)
                                           };

            // var productStockInfoQuery = _stockContext.ProductStockInfo.AsQueryable();
            // var productExtraInfoQuery = _stockContext.ProductExtraInfo.AsQueryable();

            var productInfoQuery = from p in productQuery
                                   join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                                   join pe in _stockContext.ProductExtraInfo on p.ProductId equals pe.ProductId into pse
                                   from pe in pse.DefaultIfEmpty()

                                   join pu in _stockContext.ProductUnitConversion.Where(u => u.IsDefault) on p.ProductId equals pu.ProductId into pus
                                   from pu in pus.DefaultIfEmpty()

                                   join sr in StockRemaningAsQueryable on p.ProductId equals sr.ProductId into gsr
                                   from sr in gsr.DefaultIfEmpty()
                                       //    join u in _masterDBContext.Unit on p.UnitId equals u.UnitId into gu
                                       //    from u in gu.DefaultIfEmpty()
                                   select new StockProductQuantityWarning
                                   {
                                       ProductId = p.ProductId,
                                       ProductCode = p.ProductCode,
                                       ProductName = p.ProductName,
                                       MainImageFileId = p.MainImageFileId,
                                       Specification = pe != null ? pe.Specification : string.Empty,
                                       PrimaryUnitId = p.UnitId,
                                       PrimaryUnitName = pu == null ? null : pu.ProductUnitConversionName,
                                       DecimalPlace = pu == null ? 11 : pu.DecimalPlace,
                                       //ProductTypeId = p.ProductTypeId,
                                       //ProductCateId = p.ProductCateId,
                                       AmountWarningMin = ps.AmountWarningMin ?? 0,
                                       AmountWarningMax = ps.AmountWarningMax ?? 0,
                                       TotalPrimaryQuantityRemaining = sr == null ? (decimal?)null : sr.TotalQuantityRemaning,
                                       IsReachMin = ps.AmountWarningMin >= sr.TotalQuantityRemaning,
                                       IsReachMax = ps.AmountWarningMax <= sr.TotalQuantityRemaning,
                                       //    u.UnitName
                                   };
            if (isMinOrMax.HasValue)
            {
                productInfoQuery = from p in productInfoQuery
                                   where isMinOrMax == true ? p.IsReachMin == true : p.IsReachMax == true
                                   select p;
            }

            if (rangeQuantityRemaining.Count() == 2)
            {
                productInfoQuery = from p in productInfoQuery
                                   where p.TotalPrimaryQuantityRemaining >= rangeQuantityRemaining[0] && p.TotalPrimaryQuantityRemaining <= rangeQuantityRemaining[1]
                                   select p;
            }

            if (filters != null)
            {
                productInfoQuery = productInfoQuery.InternalFilter(filters);
            }

            var total = productInfoQuery.Count();

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                productInfoQuery = productInfoQuery.SortByFieldName(sortBy, asc);
            }

            var productInfoPaged = productInfoQuery.Skip((page - 1) * size).Take(size).ToList();

            return (productInfoPaged, total);
        }

        public async Task<PageData<StockSumaryReportOutput>> StockSumaryReport(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, long beginTime, long endTime, string sortBy, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.UtcNow;

            if (beginTime > 0)
                fromDate = beginTime.UnixToDateTime().Value;

            if (endTime > 0)
                toDate = endTime.UnixToDateTime().Value;

            var productQuery = _stockContext.Product.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }
            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var productTypes = await _stockContext.ProductType.ToListAsync();

                var types = new List<int?>();
                foreach (var productTypeId in productTypeIds)
                {
                    var st = new Stack<int>();
                    st.Push(productTypeId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        types.Add(parentId);
                        var children = productTypes.Where(p => p.ParentProductTypeId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductTypeId);
                        }
                    }

                }
                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                var productCates = await _stockContext.ProductCate.ToListAsync();

                var cates = new List<int>();
                foreach (var productCateId in productCateIds)
                {
                    var st = new Stack<int>();
                    st.Push(productCateId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        cates.Add(parentId);
                        var children = productCates.Where(p => p.ParentProductCateId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductCateId);
                        }
                    }
                }
                productQuery = from p in productQuery
                               where cates.Contains(p.ProductCateId)
                               select p;
            }
            var inventories = _stockContext.Inventory.AsQueryable();

            if (stockIds != null && stockIds.Count > 0)
            {
                inventories = inventories.Where(iv => stockIds.Contains(iv.StockId));
            }

            var beforeProductQuery = (
               from iv in inventories
               join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
               join p in productQuery on d.ProductId equals p.ProductId
               where iv.IsApproved && iv.Date < fromDate
               group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId } into g
               where g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity) > 0
               select new
               {
                   BeforeProductId = (int?)g.Key.ProductId,
                   BeforeTotal = (decimal?)g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
               }
               );

            var afterProductQuery = (
                from iv in inventories
                join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                join p in productQuery on d.ProductId equals p.ProductId
                where iv.IsApproved && iv.Date >= fromDate && iv.Date <= toDate
                group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId } into g
                select new
                {
                    AfterProductId = (int?)g.Key.ProductId,
                    AfterTotalInput = (decimal?)g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : 0),
                    AfterTotalOutput = (decimal?)g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.PrimaryQuantity : 0),
                    AfterTotal = (decimal?)g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                }
                );

            var beforeIds = beforeProductQuery.Select(q => q.BeforeProductId);
            var afterIds = afterProductQuery.Select(q => q.AfterProductId);

            var report = (
                from p in _stockContext.Product.Where(p => beforeIds.Contains(p.ProductId) || afterIds.Contains(p.ProductId))
                join c in _stockContext.ProductUnitConversion.Where(c => c.IsDefault) on p.ProductId equals c.ProductId into cs
                from c in cs.DefaultIfEmpty()
                join b in beforeProductQuery on p.ProductId equals b.BeforeProductId into bp
                from b in bp.DefaultIfEmpty()
                join a in afterProductQuery on p.ProductId equals a.AfterProductId into ap
                from a in ap.DefaultIfEmpty()
                select new
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    UnitId = p.UnitId,
                    UnitName = c.ProductUnitConversionName,
                    PrimaryQualtityBefore = b == null ? 0 : b.BeforeTotal ?? 0,
                    PrimaryQualtityInput = a == null ? 0 : a.AfterTotalInput ?? 0,
                    PrimaryQualtityOutput = a == null ? 0 : a.AfterTotalOutput ?? 0,
                    PrimaryQualtityAfter = (b == null ? 0 : b.BeforeTotal ?? 0) + (a == null ? 0 : a.AfterTotal ?? 0)
                });

            var total = report.Count();

            report = report.SortByFieldName(sortBy, asc);

            var pagedData = report.OrderBy(p => p.ProductId).Skip((page - 1) * size).Take(size).ToList();

            var data = new List<StockSumaryReportOutput>();
            foreach (var item in pagedData)
            {
                data.Add(new StockSumaryReportOutput()
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    PrimaryQualtityBefore = item.PrimaryQualtityBefore,
                    PrimaryQualtityInput = item.PrimaryQualtityInput,
                    PrimaryQualtityOutput = item.PrimaryQualtityOutput,
                    PrimaryQualtityAfter = item.PrimaryQualtityAfter
                });


            }
            return (data, total);
        }
        /*
        /// <summary>
        /// Báo cáo chi tiết nhập xuất sp trong kỳ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task<StockProductDetailsReportOutput> StockProductDetailsReportBk(int productId, IList<int> stockIds, long bTime, long eTime)
        {
            if (productId <= 0)
                throw new BadRequestException(GeneralCode.InvalidParams);

            var productInfo = _stockContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            var resultData = new StockProductDetailsReportOutput
            {
                OpeningStock = null,
                Details = null
            };
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;

            if (bTime > 0)
                fromDate = bTime.UnixToDateTime().Value;

            if (eTime > 0)
                toDate = eTime.UnixToDateTime().Value;

            toDate = toDate.AddDays(1);


            DateTime? beginTime = fromDate != DateTime.MinValue ? fromDate : _stockContext.Inventory.OrderBy(q => q.Date).Select(q => q.Date).FirstOrDefault().AddDays(-1);

            #region Lấy dữ liệu tồn đầu
            var openingStockQuery = from i in _stockContext.Inventory
                                    join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                    where i.IsApproved && id.ProductId == productId
                                    select new { i, id };
            if (stockIds.Count > 0)
                openingStockQuery = openingStockQuery.Where(q => stockIds.Contains(q.i.StockId));

            if (beginTime.HasValue && beginTime != DateTime.MinValue)
                openingStockQuery = openingStockQuery.Where(q => q.i.Date < beginTime);
#if DEBUG
            var openingStockQueryDataInput = (from q in openingStockQuery
                                              where q.i.InventoryTypeId == (int)EnumInventoryType.Input
                                              group q by q.id.ProductUnitConversionId into g
                                              select new { ProductUnitConversionId = g.Key, Total = g.Sum(v => v.id.PrimaryQuantity) }).ToList();

            var openingStockQueryDataOutput = (from q in openingStockQuery
                                               where q.i.InventoryTypeId == (int)EnumInventoryType.Output
                                               group q by q.id.ProductUnitConversionId into g
                                               select new { ProductUnitConversionId = g.Key, Total = g.Sum(v => v.id.PrimaryQuantity) }).ToList();
#endif

            var openingStockQueryData = openingStockQuery.Sum(v => v.i.InventoryTypeId == (int)EnumInventoryType.Input ? v.id.PrimaryQuantity : (v.i.InventoryTypeId == (int)EnumInventoryType.Output ? -v.id.PrimaryQuantity : 0));
            #endregion
            #region Lấy dữ liệu giao dịch trong kỳ
            var inPerdiodQuery = from i in _stockContext.Inventory
                                 join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                 //join conversion in _stockContext.ProductUnitConversion on id.ProductUnitConversionId equals conversion.ProductUnitConversionId
                                 where i.IsApproved && id.ProductId == productId
                                 select new { i, id };
            if (stockIds.Count > 0)
                inPerdiodQuery = inPerdiodQuery.Where(q => stockIds.Contains(q.i.StockId));



            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
            {
                inPerdiodQuery = inPerdiodQuery.Where(q => q.i.Date >= fromDate && q.i.Date < toDate);
            }
            else
            {
                if (fromDate != DateTime.MinValue)
                {
                    inPerdiodQuery = inPerdiodQuery.Where(q => q.i.Date >= fromDate);
                }
                if (toDate != DateTime.MinValue)
                {
                    inPerdiodQuery = inPerdiodQuery.Where(q => q.i.Date < toDate);
                }
            }

            var totalRecord = inPerdiodQuery.Count();
            var inPeriodData = inPerdiodQuery.AsNoTracking().Select(q => new
            {
                InventoryId = q.i.InventoryId,
                q.i.StockId,
                IssuedDate = q.i.Date,
                q.i.CreatedDatetimeUtc,
                InventoryCode = q.i.InventoryCode,
                InventoryTypeId = q.i.InventoryTypeId,
                Description = q.i.Content,
                InventoryDetailId = q.id.InventoryDetailId,
                RefObjectCode = q.id.RefObjectCode,
                PrimaryQuantity = q.id.PrimaryQuantity,
                //SecondaryUnitId = q.conversion.SecondaryUnitId,
                SecondaryQuantity = q.id.ProductUnitConversionQuantity,
                ProductUnitConversionId = q.id.ProductUnitConversionId
            }).ToList();

            var productUnitConversionIdsList = inPeriodData.Where(q => q.ProductUnitConversionId > 0).Select(q => q.ProductUnitConversionId).ToList();
            var productUnitConversionData = _stockContext.ProductUnitConversion.AsNoTracking().Where(q => productUnitConversionIdsList.Contains(q.ProductUnitConversionId)).ToList();
            var unitData = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

            resultData.OpeningStock = new List<OpeningStockProductModel>
                {
                    new OpeningStockProductModel
                    {
                        PrimaryUnitId = productInfo.UnitId,
                        Total = openingStockQueryData
                    }
                };

            resultData.Details = new List<StockProductDetailsModel>(totalRecord);

            var stocks = await _stockContext.Stock.AsNoTracking().ToListAsync();
            foreach (var item in inPeriodData.OrderBy(q => q.IssuedDate).ThenBy(q => q.CreatedDatetimeUtc).ToList())
            {
                var productUnitConversionObj = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId);
                var secondaryUnitObj = unitData.FirstOrDefault(q => q.UnitId == item.ProductUnitConversionId);
                var secondaryUnitName = secondaryUnitObj != null ? secondaryUnitObj.UnitName : string.Empty;
                var secondaryUnitId = secondaryUnitObj != null ? (int?)secondaryUnitObj.UnitId : null;
                resultData.Details.Add(new StockProductDetailsModel
                {
                    InventoryId = item.InventoryId,
                    StockId = item.StockId,
                    StockName = stocks.FirstOrDefault(s => s.StockId == item.StockId)?.StockName,
                    IssuedDate = item.IssuedDate.GetUnix(),
                    InventoryCode = item.InventoryCode,
                    InventoryTypeId = item.InventoryTypeId,
                    Description = item.Description,
                    SecondaryUnitName = secondaryUnitName,
                    InventoryDetailId = item.InventoryDetailId,
                    RefObjectCode = item.RefObjectCode,
                    PrimaryUnitId = productInfo.UnitId,
                    PrimaryQuantity = item.PrimaryQuantity,
                    SecondaryUnitId = secondaryUnitId,
                    SecondaryQuantity = item.SecondaryQuantity,
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    ProductUnitConversion = productUnitConversionObj
                });
            }
            #endregion

            return resultData;
        }
        */
        public async Task<StockProductDetailsReportOutput> StockProductDetailsReport(int productId, IList<int> stockIds, long bTime, long eTime)
        {
            if (productId <= 0)
                throw new BadRequestException(GeneralCode.InvalidParams);

            var productInfo = _stockContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            var resultData = new StockProductDetailsReportOutput
            {
                OpeningStock = null,
                Details = null
            };
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;

            if (bTime > 0)
                fromDate = bTime.UnixToDateTime().Value;

            if (eTime > 0)
                toDate = eTime.UnixToDateTime().Value;



            DateTime? beginTime = fromDate != DateTime.MinValue ? fromDate : _stockContext.Inventory.OrderBy(q => q.Date).Select(q => q.Date).FirstOrDefault().AddDays(-1);

            #region Lấy dữ liệu tồn đầu

            var inventories = _stockContext.Inventory.AsQueryable();

            if (stockIds.Count > 0)
                inventories = inventories.Where(q => stockIds.Contains(q.StockId));

            if (beginTime.HasValue && beginTime != DateTime.MinValue)
                inventories = inventories.Where(q => q.Date < beginTime);

            var inventoryDetails = (
                from i in inventories
                join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                where i.IsApproved && id.ProductId == productId
                select new
                {
                    i.InventoryTypeId,
                    id.ProductUnitConversionId,
                    id.ProductUnitConversionQuantity,
                    id.PrimaryQuantity
                });

            var openingStockQuery = (
                from id in inventoryDetails
                group id by id.ProductUnitConversionId into pu
                select new SumQuantity
                {
                    ProductUnitConversionId = pu.Key,
                    TotalPrimaryUnit = pu.Sum(v => v.InventoryTypeId == (int)EnumInventoryType.Input ? v.PrimaryQuantity : -v.PrimaryQuantity),
                    TotalPu = pu.Sum(v => v.InventoryTypeId == (int)EnumInventoryType.Input ? v.ProductUnitConversionQuantity : -v.ProductUnitConversionQuantity),
                }).ToList();

            #endregion
            #region Lấy dữ liệu giao dịch trong kỳ

            var inPerdiodInventories = _stockContext.Inventory.AsQueryable();

            if (stockIds.Count > 0)
                inPerdiodInventories = inPerdiodInventories.Where(q => stockIds.Contains(q.StockId));

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
            {
                inPerdiodInventories = inPerdiodInventories.Where(q => q.Date >= fromDate && q.Date <= toDate);
            }
            else
            {
                if (fromDate != DateTime.MinValue)
                {
                    inPerdiodInventories = inPerdiodInventories.Where(q => q.Date >= fromDate);
                }
                if (toDate != DateTime.MinValue)
                {
                    inPerdiodInventories = inPerdiodInventories.Where(q => q.Date <= toDate);
                }
            }

            var inPeriodData = (
              from i in inPerdiodInventories
              join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
              where i.IsApproved && id.ProductId == productId
              select new
              {
                  i.InventoryId,
                  i.CustomerId,
                  i.DepartmentId,
                  i.StockId,
                  i.Date,
                  i.CreatedDatetimeUtc,
                  i.InventoryCode,
                  i.InventoryTypeId,
                  i.Content,
                  id.InventoryDetailId,
                  id.RefObjectCode,
                  id.PrimaryQuantity,
                  id.ProductUnitConversionQuantity,
                  id.ProductUnitConversionId,
                  id.OrderCode,
                  id.ProductionOrderCode,
                  id.Pocode
              }).ToList();

            var productUnitConversionIdsList = inPeriodData.Where(q => q.ProductUnitConversionId > 0).Select(q => q.ProductUnitConversionId).ToList();
            var productUnitConversionData = _stockContext.ProductUnitConversion.AsNoTracking().Where(q => productUnitConversionIdsList.Contains(q.ProductUnitConversionId)).ToList();
            var unitData = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

            resultData.OpeningStock = new List<OpeningStockProductModel>
                {
                    new OpeningStockProductModel
                    {
                        PrimaryUnitId = productInfo.UnitId,
                        Total = openingStockQuery.Sum(o=>o.TotalPrimaryUnit)
                    }
                };

            resultData.Details = new List<StockProductDetailsModel>();

            var stocks = await _stockContext.Stock.AsNoTracking().ToListAsync();
            var totalByTimes = openingStockQuery.ToDictionary(o => o.ProductUnitConversionId, o => o);

            var totalPrimary = openingStockQuery.Sum(o => o.TotalPrimaryUnit);

            foreach (var item in inPeriodData.OrderBy(q => q.Date).ThenBy(q => q.InventoryTypeId).ThenBy(q => q.InventoryDetailId).ToList())
            {
                var productUnitConversionObj = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId);
                var secondaryUnitObj = unitData.FirstOrDefault(q => q.UnitId == item.ProductUnitConversionId);
                var secondaryUnitName = secondaryUnitObj != null ? secondaryUnitObj.UnitName : string.Empty;
                var secondaryUnitId = secondaryUnitObj != null ? (int?)secondaryUnitObj.UnitId : null;

                if (!totalByTimes.ContainsKey(item.ProductUnitConversionId))
                {
                    totalByTimes.Add(item.ProductUnitConversionId, new SumQuantity() { ProductUnitConversionId = item.ProductUnitConversionId, TotalPrimaryUnit = 0, TotalPu = 0 });
                }

                var currentSum = totalByTimes[item.ProductUnitConversionId];

                if (item.InventoryTypeId == (int)EnumInventoryType.Input)
                {
                    currentSum.Add(item.PrimaryQuantity, item.ProductUnitConversionQuantity);
                    totalPrimary += item.PrimaryQuantity;
                }
                else
                {
                    currentSum.Sub(item.PrimaryQuantity, item.ProductUnitConversionQuantity);

                    totalPrimary = totalPrimary.SubDecimal(item.PrimaryQuantity);
                }

                resultData.Details.Add(new StockProductDetailsModel
                {
                    InventoryId = item.InventoryId,
                    CustomerId = item.CustomerId,
                    DepartmentId = item.DepartmentId,
                    StockId = item.StockId,
                    StockName = stocks.FirstOrDefault(s => s.StockId == item.StockId)?.StockName,
                    IssuedDate = item.Date.GetUnix(),
                    InventoryCode = item.InventoryCode,
                    InventoryTypeId = item.InventoryTypeId,
                    Description = item.Content,
                    SecondaryUnitName = secondaryUnitName,
                    InventoryDetailId = item.InventoryDetailId,
                    RefObjectCode = item.RefObjectCode,
                    PrimaryUnitId = productInfo.UnitId,
                    PrimaryQuantity = item.PrimaryQuantity,
                    SecondaryUnitId = secondaryUnitId,
                    SecondaryQuantity = item.ProductUnitConversionQuantity,
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    ProductUnitConversion = productUnitConversionObj,
                    EndPrimaryQuantityForAllPus = totalPrimary,
                    EndOfPerdiodPrimaryQuantity = currentSum.TotalPrimaryUnit,
                    EndOfPerdiodProductUnitConversionQuantity = currentSum.TotalPu,
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Pocode = item.Pocode
                });
            }
            #endregion

            return resultData;
        }



        /// <summary>
        /// Báo cáo tổng hợp NXT 2 DVT 2 DVT (SỐ LƯỢNG) - Mẫu báo cáo kho 03
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm: mã sp, tên sp</param>
        /// <param name="stockIds">Danh sách id kho cần báo cáo</param>
        /// <param name="bTime"></param>
        /// <param name="eTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<StockSumaryReportForm03Output>> StockSumaryReportProductUnitConversionQuantity(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, long bTime, long eTime, int page = 1, int size = int.MaxValue)
        {
            keyword = (keyword ?? "").Trim();

            IList<int> allowStockIds = stockIds?.Where(stockId => _currentContextService.StockIds.Contains(stockId))?.ToList();
            if (stockIds == null || stockIds.Count == 0)
            {
                allowStockIds = _currentContextService.StockIds;
            }

            var data = (await _stockContext.ExecuteDataProcedure("asp_Inventory_ReportProductConversion",
                new[] {
                    new SqlParameter("@Keyword",keyword),
                    allowStockIds.ToSqlParameter("@StockIds"),
                    productTypeIds.ToSqlParameter("@ProductTypeIds"),
                    productCateIds.ToSqlParameter("@ProductCateIds"),
                    new SqlParameter("@FromDate",bTime.UnixToDateTime()),
                    new SqlParameter("@ToDate",eTime.UnixToDateTime()),
                    new SqlParameter("@Page",page),
                    new SqlParameter("@Size",size),
                })
                ).ConvertData<StockSumaryReportForm3Data>();

            var lstData = new List<StockSumaryReportForm03Output>();
            var groupData = data.GroupBy(d => new { d.ProductId, d.ProductCode, d.ProductName, d.UnitId }).ToList();
            foreach (var g in groupData)
            {
                lstData.Add(new StockSumaryReportForm03Output()
                {
                    RankNumber = g.FirstOrDefault()?.RankNumber ?? 0,
                    ProductId = g.Key.ProductId,
                    ProductCode = g.Key.ProductCode,
                    ProductName = g.Key.ProductName,
                    UnitId = g.Key.UnitId,

                    // UnitName = "",

                    SumPrimaryQuantityBefore = g.Sum(p => p.StartPrimaryRemaing) ?? 0,
                    SumPrimaryQuantityInput = g.Sum(p => p.InPrimary) ?? 0,
                    SumPrimaryQuantityOutput = g.Sum(p => p.OutPrimaryRemaing) ?? 0,
                    SumPrimaryQuantityAfter = g.Sum(p => p.PrimaryRemaing) ?? 0,

                    PakageIdNotExport = 0,
                    PakageCodeNotExport = "",

                    PakageDateNotExport = g.Min(p => p.MaxInputDate).GetUnix() ?? 0,

                    ProductAltSummaryList = g.Select(p => new ProductAltSummary()
                    {
                        RankNumber = p.RankNumber,
                        ProductId = g.Key.ProductId,

                        //UnitId = 0,
                        PrimaryQuantityBefore = p.StartPrimaryRemaing ?? 0,
                        PrimaryQuantityInput = p.InPrimary ?? 0,
                        PrimaryQuantityOutput = p.OutPrimaryRemaing ?? 0,
                        PrimaryQuantityAfter = p.PrimaryRemaing ?? 0,


                        ProductUnitConversionId = p.ProductUnitConversionId,
                        ProductUnitCoversionName = p.ProductUnitConversionName,

                        //ConversionDescription = "",

                        ProductUnitConversionQuantityBefore = p.StartProductUnitConversionRemaining ?? 0,
                        ProductUnitConversionQuantityInput = p.InProductUnitConversion ?? 0,
                        ProductUnitConversionQuantityOutput = p.OutProductUnitConversion ?? 0,
                        ProductUnitConversionQuantityAfter = p.ProductUnitConversionRemaining ?? 0
                    }).ToList()
                });
            }
            return (lstData, Convert.ToInt32(data.FirstOrDefault()?.TotalRecord ?? 0));

            /*
            DateTime fromDate = DateTime.Now.AddMonths(-3);
            DateTime toDate = DateTime.Now;

            if (bTime > 0)
                fromDate = bTime.UnixToDateTime().Value;

            if (eTime > 0)
                toDate = eTime.UnixToDateTime().Value;

            var productQuery = _stockContext.Product.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }
            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var productTypes = await _stockContext.ProductType.ToListAsync();

                var types = new List<int?>();
                foreach (var productTypeId in productTypeIds)
                {
                    var st = new Stack<int>();
                    st.Push(productTypeId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        types.Add(parentId);
                        var children = productTypes.Where(p => p.ParentProductTypeId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductTypeId);
                        }
                    }

                }
                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                var productCates = await _stockContext.ProductCate.ToListAsync();

                var cates = new List<int>();
                foreach (var productCateId in productCateIds)
                {
                    var st = new Stack<int>();
                    st.Push(productCateId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        cates.Add(parentId);
                        var children = productCates.Where(p => p.ParentProductCateId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductCateId);
                        }
                    }
                }
                productQuery = from p in productQuery
                               where cates.Contains(p.ProductCateId)
                               select p;
            }

            var inventoryQuery = _stockContext.Inventory.AsQueryable();

            if (stockIds != null && stockIds.Count > 0)
            {
                inventoryQuery = inventoryQuery.Where(iv => stockIds.Contains(iv.StockId));
            }

            var befores = await (
                   from iv in inventoryQuery
                   join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                   join p in productQuery on d.ProductId equals p.ProductId
                   where iv.IsApproved && iv.Date < fromDate
                   group new { d.PrimaryQuantity, iv.InventoryTypeId } by d.ProductId into g

                   select new
                   {
                       ProductId = g.Key,
                       Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                   }
           ).Where(b => b.Total < -Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER || b.Total > Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER)
           .ToListAsync();

            var afters = await (
                from iv in inventoryQuery
                join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                join p in productQuery on d.ProductId equals p.ProductId
                where iv.IsApproved && iv.Date >= fromDate && iv.Date <= toDate

                group new { d.PrimaryQuantity, iv.InventoryTypeId } by d.ProductId into g
                select new
                {
                    ProductId = g.Key,
                    TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : 0),
                    TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.PrimaryQuantity : 0),
                    Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                }
           ).ToListAsync();


            var beforesByAltUnit = await (
                 from iv in inventoryQuery
                 join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                 join p in productQuery on d.ProductId equals p.ProductId
                 where iv.IsApproved && iv.Date < fromDate
                 group new { d.ProductUnitConversionQuantity, d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId, d.ProductUnitConversionId } into g
                 select new
                 {
                     g.Key.ProductId,
                     g.Key.ProductUnitConversionId,
                     Primary_Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),
                     Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
                 }
            ).Where(b => b.Total < -Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER || b.Total > Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER)
            .ToListAsync();

            var aftersByAltUnit = await (
                   from iv in inventoryQuery
                   join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                   join p in productQuery on d.ProductId equals p.ProductId
                   where iv.IsApproved && iv.Date >= fromDate && iv.Date <= toDate
                   group new { d.ProductUnitConversionQuantity, d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId, d.ProductUnitConversionId } into g
                   select new
                   {
                       g.Key.ProductId,
                       g.Key.ProductUnitConversionId,

                       Primary_TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : 0),
                       Primary_TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.PrimaryQuantity : 0),
                       Primary_Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),

                       TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : 0),
                       TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.ProductUnitConversionQuantity : 0),
                       Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
                   }
               ).ToListAsync();

            #region product  - ProductUnitModel  
            var productAltUnitModelList = new List<ProductAltUnitModel>();
            var productIds = befores.Select(b => b.ProductId).Distinct().ToHashSet();

            foreach (var a in afters)
            {
                if (!productIds.Contains(a.ProductId))
                    productIds.Add(a.ProductId);
            }

            foreach (var b in beforesByAltUnit)
            {
                if (!productAltUnitModelList.Any(p => p.ProductId == b.ProductId && p.ProductUnitConversionId == b.ProductUnitConversionId) && b.ProductUnitConversionId > 0)
                {
                    productAltUnitModelList.Add(new ProductAltUnitModel()
                    {
                        ProductId = b.ProductId,
                        ProductUnitConversionId = b.ProductUnitConversionId
                    });
                }
            }

            foreach (var a in aftersByAltUnit)
            {
                if (!productAltUnitModelList.Any(p => p.ProductId == a.ProductId && p.ProductUnitConversionId == a.ProductUnitConversionId) && a.ProductUnitConversionId > 0)
                {
                    productAltUnitModelList.Add(new ProductAltUnitModel()
                    {
                        ProductId = a.ProductId,
                        ProductUnitConversionId = a.ProductUnitConversionId
                    });
                }
            }

            #endregion

            var total = productIds.Count;
            var productPaged = productIds.Skip((page - 1) * size).Take(size);

            var productInfos = await _stockContext.Product.Where(p => productPaged.Contains(p.ProductId)).AsNoTracking().ToListAsync();
            var productUnitConversionInfos = await _stockContext.ProductUnitConversion.Where(p => productPaged.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            var unitIds = productInfos.Select(p => p.UnitId).ToList();
            var unitInfos = await _masterDBContext.Unit.Where(p => unitIds.Contains(p.UnitId)).Select(q => new { q.UnitId, q.UnitName }).ToListAsync();

            var productAltUnitPaged = productAltUnitModelList.Where(q => productPaged.Contains(q.ProductId)).ToList();

            var packageQuery = _stockContext.Package.AsQueryable();
            if (stockIds != null && stockIds.Count > 0)
            {
                packageQuery = packageQuery.Where(q => stockIds.Contains(q.StockId));
            }

            var packageList = packageQuery.ToList();
            var packageInfoData = (from pkg in packageList
                                   join p in productInfos on pkg.ProductId equals p.ProductId
                                   //join id in _stockContext.InventoryDetail on p.ProductId equals id.InventoryId
                                   //join i in inventoryQuery.Where(q => (q.Date > fromDate && q.Date <= toDate) && q.IsApproved) on id.InventoryId equals i.InventoryId
                                   select new
                                   {
                                       //i.InventoryId,
                                       //i.InventoryCode,
                                       //i.InventoryTypeId,
                                       //i.Date,
                                       p.ProductId,
                                       pkg.PackageId,
                                       pkg.PackageCode,
                                       pkg.Date,
                                       p.UnitId,
                                       pkg.PrimaryQuantityRemaining,
                                       pkg.ProductUnitConversionId,
                                       pkg.ProductUnitConversionRemaining
                                   }).ToList();

            var productAltSummaryData = (
                 from p in productAltUnitPaged
                 join c in productUnitConversionInfos on p.ProductUnitConversionId equals c.ProductUnitConversionId
                 //join info in productInfos on p.ProductId equals info.ProductId
                 join b in beforesByAltUnit on new { p.ProductId, p.ProductUnitConversionId } equals new { b.ProductId, b.ProductUnitConversionId } into bp
                 from b in bp.DefaultIfEmpty()
                 join a in aftersByAltUnit on new { p.ProductId, p.ProductUnitConversionId } equals new { a.ProductId, a.ProductUnitConversionId } into ap
                 from a in ap.DefaultIfEmpty()
                 select new ProductAltSummary
                 {
                     ProductId = p.ProductId,

                     PrimaryQuantityBefore = b == null ? 0 : b.Primary_Total,
                     PrimaryQuantityInput = a == null ? 0 : a.Primary_TotalInput,
                     PrimaryQuantityOutput = a == null ? 0 : a.Primary_TotalOutput,
                     PrimaryQuantityAfter = (b == null ? 0 : b.Primary_Total) + (a == null ? 0 : a.Primary_Total),


                     ProductUnitConversionId = p.ProductUnitConversionId,
                     UnitId = c.SecondaryUnitId,
                     ProductUnitCoversionName = c.ProductUnitConversionName,
                     //ConversionDescription = c.ConversionDescription,
                     ProductUnitConversionQuantityBefore = b == null ? 0 : b.Total,
                     ProductUnitConversionQuantityInput = a == null ? 0 : a.TotalInput,
                     ProductUnitConversionQuantityOutput = a == null ? 0 : a.TotalOutput,
                     ProductUnitConversionQuantityAfter = (b == null ? 0 : b.Total) + (a == null ? 0 : a.Total)
                 }
             ).ToList();

            var resultData = (
                from p in productInfos
                join u in unitInfos on p.UnitId equals u.UnitId
                join b in befores on p.ProductId equals b.ProductId into bp
                from b in bp.DefaultIfEmpty()
                join a in afters on p.ProductId equals a.ProductId into ap
                from a in ap.DefaultIfEmpty()
                select new StockSumaryReportForm03Output
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                   // UnitId = p.UnitId,
                   // UnitName = u.UnitName,
                    SumPrimaryQuantityBefore = b == null ? 0 : b.Total,
                    SumPrimaryQuantityInput = a == null ? 0 : a.TotalInput,
                    SumPrimaryQuantityOutput = a == null ? 0 : a.TotalOutput,
                    SumPrimaryQuantityAfter = (b == null ? 0 : b.Total) + (a == null ? 0 : a.Total),
                    ProductAltSummaryList = productAltSummaryData.Where(q => q.ProductId == p.ProductId).ToList()
                }
            ).ToList();

            if (packageInfoData != null && packageInfoData.Count > 0)
            {
                var pakageExportIdsList = (from id in _stockContext.InventoryDetail
                                           join i in _stockContext.Inventory.Where(q => q.IsApproved && q.InventoryTypeId == (int)EnumInventoryType.Output && (q.Date > fromDate && q.Date < toDate)) on id.InventoryId equals i.InventoryId
                                           where id.FromPackageId > 0 && productIds.Contains(id.ProductId)
                                           select new { id.FromPackageId }
                                          ).ToList();
                var pakageExportIds = pakageExportIdsList.Select(q => Convert.ToInt64(q.FromPackageId)).ToList();
                foreach (var item in resultData)
                {

                    var pakageNotExport = packageInfoData.Where(q => q.ProductId == item.ProductId && !pakageExportIds.Contains(q.PackageId)).OrderBy(q => q.Date).FirstOrDefault();

                    item.PakageIdNotExport = pakageNotExport != null ? pakageNotExport.PackageId : 0;
                    item.PakageCodeNotExport = pakageNotExport != null ? pakageNotExport.PackageCode : string.Empty;
                    item.PakageDateNotExport = pakageNotExport != null ? (pakageNotExport.Date.HasValue ? ((DateTime)pakageNotExport.Date).GetUnix() : 0) : 0;
                }
            }

            return new PageData<StockSumaryReportForm03Output>
            {
                List = resultData,
                Total = total
            };
            */
        }

        /// <summary>
        /// Nhật ký nhập xuất kho - Mẫu báo cáo kho 04
        /// </summary>
        /// <param name="stockIds">Danh sách id kho cần báo cáo</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<StockSumaryReportForm04Output>> StockSumaryReportForm04(IList<int> stockIds, long beginTime, long endTime, int page = 1, int size = int.MaxValue)
        {

            var bTime = DateTime.MinValue;
            var eTime = DateTime.MinValue;

            if (beginTime > 0)
            {
                bTime = beginTime.UnixToDateTime().Value;
            }
            if (endTime > 0)
            {
                eTime = endTime.UnixToDateTime().Value;
            }

            var inventoryQuery = _stockContext.Inventory.AsNoTracking().Where(q => q.IsApproved);
            if (stockIds != null && stockIds.Count > 0)
            {
                inventoryQuery = inventoryQuery.Where(iv => stockIds.Contains(iv.StockId));
            }
            if (bTime != DateTime.MinValue && eTime != DateTime.MinValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.Date >= bTime && q.Date <= eTime);
            }
            else
            {
                if (bTime != DateTime.MinValue)
                {
                    inventoryQuery = inventoryQuery.Where(q => q.Date >= bTime);
                }
                if (eTime != DateTime.MinValue)
                {
                    inventoryQuery = inventoryQuery.Where(q => q.Date <= eTime);
                }
            }
            inventoryQuery = inventoryQuery.OrderByDescending(q => q.Date);
            var total = inventoryQuery.Count();
            var inventoryDataList = inventoryQuery.Skip((page - 1) * size).Take(size).ToList();
            var inventoryIds = inventoryDataList.Select(q => q.InventoryId).ToList();
            var customerIds = inventoryDataList.Select(q => q.CustomerId).ToList();

            var customerDataList = await _organizationDBContext.Customer.Where(q => customerIds.Contains(q.CustomerId)).Select(q => new { q.CustomerId, q.CustomerCode, q.CustomerName }).ToListAsync();
            var inventoryDetailsData = _stockContext.InventoryDetail.AsNoTracking().Where(q => inventoryIds.Contains(q.InventoryId)).ToList();
            var productIds = inventoryDetailsData.Select(q => q.ProductId).ToList();

            var productDataList = _stockContext.Product.Where(q => productIds.Contains(q.ProductId)).Select(q => new { q.ProductId, q.ProductCode, q.ProductName, q.UnitId }).ToList();
            var unitIds = productDataList.Select(q => q.UnitId).ToList();
            var unitDataList = await _masterDBContext.Unit.Where(q => unitIds.Contains(q.UnitId)).Select(q => new { q.UnitId, q.UnitName }).ToListAsync();

            var createdUserIdsList = inventoryDataList.Select(q => q.CreatedByUserId).ToList();
            var updatedUserIdsList = inventoryDataList.Select(q => q.UpdatedByUserId).ToList();
            var userIdsList = createdUserIdsList.Concat(updatedUserIdsList).ToList();
            var userDataList = await _masterDBContext.User.Where(q => userIdsList.Contains(q.UserId)).ToListAsync();

            var reportInventoryDetailsOutputModelList = new List<ReportForm04InventoryDetailsOutputModel>(inventoryDetailsData.Count) { };
            foreach (var inventoryDetail in inventoryDetailsData)
            {
                var productInfo = productDataList.FirstOrDefault(q => q.ProductId == inventoryDetail.ProductId);

                var item = new ReportForm04InventoryDetailsOutputModel
                {
                    InventoryDetailId = inventoryDetail.InventoryDetailId,
                    InventoryId = inventoryDetail.InventoryId,
                    ProductId = inventoryDetail.ProductId,
                    ProductCode = productInfo?.ProductCode ?? string.Empty,
                    ProductName = productInfo?.ProductName ?? string.Empty,
                    PrimaryUnitId = productInfo?.UnitId ?? 0,
                    UnitName = unitDataList.FirstOrDefault(q => q.UnitId == productInfo?.UnitId)?.UnitName ?? string.Empty,
                    ProductUnitConversionId = inventoryDetail.ProductUnitConversionId,
                    PrimaryQuantity = inventoryDetail.PrimaryQuantity,
                    UnitPrice = inventoryDetail.UnitPrice,
                    POCode = inventoryDetail.Pocode ?? string.Empty,
                    ProductionOrderCode = inventoryDetail.ProductionOrderCode ?? string.Empty,
                    OrderCode = inventoryDetail.OrderCode ?? string.Empty
                };

                reportInventoryDetailsOutputModelList.Add(item);
            }

            var reportInventoryOutputModelList = new List<StockSumaryReportForm04Output>(inventoryDataList.Count) { };
            foreach (var inventory in inventoryDataList)
            {
                var item = new StockSumaryReportForm04Output()
                {
                    InventoryDetailsOutputModel = new List<ReportForm04InventoryDetailsOutputModel>()
                };
                item.InventoryId = inventory.InventoryId;
                item.InventoryCode = inventory.InventoryCode;
                item.BillCode = inventory.BillCode;
                item.DateUtc = inventory.Date.GetUnix();
                item.InventoryTypeId = inventory.InventoryTypeId;
                item.Content = inventory.Content;
                item.CustomerId = inventory.CustomerId;
                item.CustomerCode = customerDataList.FirstOrDefault(q => q.CustomerId == inventory.CustomerId)?.CustomerCode ?? string.Empty;
                item.CustomerName = customerDataList.FirstOrDefault(q => q.CustomerId == inventory.CustomerId)?.CustomerName ?? string.Empty;
                item.CreatedByUserId = inventory.CreatedByUserId;
                item.CreatedByUserName = userDataList.FirstOrDefault(q => q.UserId == inventory.CreatedByUserId)?.UserName ?? string.Empty;
                item.CreatedDatetimeUtc = inventory.CreatedDatetimeUtc.GetUnix();
                item.UpdatedByUserId = inventory.UpdatedByUserId;
                item.UpdatedByUserName = userDataList.FirstOrDefault(q => q.UserId == inventory.UpdatedByUserId)?.UserName ?? string.Empty;
                item.UpdatedDatetimeUtc = inventory.UpdatedDatetimeUtc.GetUnix();
                item.Censor = userDataList.FirstOrDefault(q => q.UserId == inventory.CensorByUserId)?.UserName ?? string.Empty;
                item.CensorDate = inventory.CensorDatetimeUtc.GetUnix();

                item.InventoryDetailsOutputModel = reportInventoryDetailsOutputModelList.Where(q => q.InventoryId == inventory.InventoryId).ToList();
                reportInventoryOutputModelList.Add(item);
            }

            return new PageData<StockSumaryReportForm04Output> { List = reportInventoryOutputModelList, Total = total };

        }

        #region Private Methods



        #endregion

        //private class ProductAltUnitModel
        //{
        //    public int ProductId { get; set; }

        //    public int ProductUnitConversionId { get; set; }
        //}

        class SumQuantity
        {
            public int ProductUnitConversionId { get; set; }
            public decimal TotalPrimaryUnit { get; set; }
            public decimal TotalPu { get; set; }

            public void Add(decimal primaryQuantity, decimal puQuantity)
            {
                TotalPrimaryUnit = TotalPrimaryUnit.AddDecimal(primaryQuantity);
                TotalPu = TotalPu.AddDecimal(puQuantity);
            }

            public void Sub(decimal primaryQuantity, decimal puQuantity)
            {
                TotalPrimaryUnit = TotalPrimaryUnit.SubDecimal(primaryQuantity);
                TotalPu = TotalPu.SubDecimal(puQuantity);
            }
        }
    }
}
