using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.ManufacturingDB;
using TargetProductivityEntity = VErp.Infrastructure.EF.ManufacturingDB.TargetProductivity;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Manafacturing.Service.Facade
{
    internal class ProductivityWorkloadFacade
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IProductHelperService _productHelperService;
        public ProductivityWorkloadFacade(ManufacturingDBContext manufacturingDBContext, IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDBContext;
            _productHelperService = productHelperService;
        }

        public async Task<(Dictionary<int, ProductTargetProductivityByStep> productTargets, Dictionary<long, ProductTargetProductivityByStep> semiTargets)> GetProductivities(List<int> productIds, List<long> semiIds)
        {
            var semis = await _manufacturingDBContext.ProductSemi.AsNoTracking()
                .Where(s => semiIds.Contains(s.ProductSemiId))
                .ToListAsync();

            var semiToProduct = semis.ToDictionary(s => s.ProductSemiId, s => s.RefProductId);

            productIds.AddRange(semis.Where(s => s.RefProductId.HasValue).Select(s => (int)s.RefProductId.Value));

            var productInfos = await _productHelperService.GetListProducts(productIds);

            var targetProductivityIds = productInfos.Where(p => p.TargetProductivity.HasValue).Select(p => p.TargetProductivity.Value).ToList();

            var targetProductivityInfos = (await _manufacturingDBContext.TargetProductivity.Include(t => t.TargetProductivityDetail)
                .Where(t => targetProductivityIds.Contains(t.TargetProductivityId) || t.IsDefault)
                .ToListAsync()
                ).ToDictionary(t => t.TargetProductivityId,
                    t => new TargetModel
                    {
                        Info = t,
                        BySteps = t.TargetProductivityDetail.GroupBy(d => d.ProductionStepId)
                                           .ToDictionary(d => d.Key, d => d.First())
                    });

            var productByIds = productInfos.ToDictionary(p => p.ProductId, p => p);


            var semiTarget = new Dictionary<long, ProductTargetProductivityByStep>();
            var productTarget = new Dictionary<int, ProductTargetProductivityByStep>();

            var defaultProductivity = targetProductivityInfos.FirstOrDefault(t => t.Value.Info.IsDefault).Value;

            var objects = new List<LinkDataObjectModel>();

            objects.AddRange(productIds.Select(p => new LinkDataObjectModel
            {
                ObjectTypeId = EnumProductionStepLinkDataObjectType.Product,
                ObjectId = p
            }));
            objects.AddRange(semiIds.Select(p => new LinkDataObjectModel
            {
                ObjectTypeId = EnumProductionStepLinkDataObjectType.ProductSemi,
                ObjectId = p
            }));

            foreach (var obj in objects)
            {

                int? productId = null;
                if (obj.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {
                    semiToProduct.TryGetValue(obj.ObjectId, out var pId);
                    if (pId > 0)
                        productId = (int?)pId;
                }
                else
                {
                    productId = (int)obj.ObjectId;
                }

                TargetModel targetProductivityInfo = null;

                decimal? productPurity = null;
                if (productId.HasValue
                    && productByIds.TryGetValue(productId.Value, out var productInfo))
                {
                    productPurity = productInfo.ProductPurity;
                    targetProductivityInfos.TryGetValue(productInfo.TargetProductivity ?? targetProductivityInfos.FirstOrDefault(t => t.Value.Info.IsDefault).Key, out targetProductivityInfo);
                }

                if (targetProductivityInfo == null)
                {
                    targetProductivityInfo = defaultProductivity;
                }

                var target = new ProductTargetProductivityByStep();
                if (targetProductivityInfo != null)
                {
                    foreach (var (step, detail) in targetProductivityInfo.BySteps)
                    {
                        decimal? rate = null;

                        var workloadType = (EnumWorkloadType)detail.WorkLoadTypeId;
                        switch (workloadType)
                        {
                            case EnumWorkloadType.Quantity:
                                rate = 1;
                                break;
                            case EnumWorkloadType.Purity:
                                rate = productPurity ?? 1;
                                break;
                        }

                        target.Add(step, new ProductStepTargetProductivityDetail()
                        {
                            TargetProductivityId = targetProductivityInfo.Info.TargetProductivityId,

                            EstimateProductionDays = targetProductivityInfo.Info.EstimateProductionDays,
                            EstimateProductionQuantity = targetProductivityInfo.Info.EstimateProductionQuantity,

                            TargetProductivity = detail.TargetProductivity,
                            ProductionStepId = detail.ProductionStepId,
                            ProductivityTimeTypeId = (EnumProductivityTimeType)detail.ProductivityTimeTypeId,
                            ProductivityResourceTypeId = (EnumProductivityResourceType)detail.ProductivityResourceTypeId,

                            WorkLoadTypeId = workloadType,

                            Rate = rate ?? 1
                        });
                    }

                }

                if (obj.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {

                    semiTarget.TryAdd(obj.ObjectId, target);
                }
                else
                {
                    productTarget.TryAdd((int)obj.ObjectId, target);
                }

            }
            return (productTarget, semiTarget);

        }

        class LinkDataObjectModel
        {
            public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
            public long ObjectId { get; set; }
        }

        private class TargetModel
        {
            public TargetProductivityEntity Info { get; set; }
            public Dictionary<int, TargetProductivityDetail> BySteps { get; set; }
        }

        public class ProductTargetProductivityByStep : Dictionary<int, ProductStepTargetProductivityDetail>
        {

        }

        public class ProductStepTargetProductivityDetail
        {
            public int TargetProductivityId { get; set; }

            public decimal? EstimateProductionDays { get; set; }
            public decimal? EstimateProductionQuantity { get; set; }

            public decimal TargetProductivity { get; set; }
            public int ProductionStepId { get; set; }
            public EnumProductivityTimeType ProductivityTimeTypeId { get; set; }
            public EnumProductivityResourceType ProductivityResourceTypeId { get; set; }

            public EnumWorkloadType WorkLoadTypeId { get; set; }

            public decimal? Rate { get; set; }
        }
    }
}
