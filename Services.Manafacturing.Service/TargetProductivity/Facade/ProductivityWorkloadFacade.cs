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

        public async Task<(Dictionary<int, LinkDataObjectTargetProductivity> productTargets, Dictionary<long, LinkDataObjectTargetProductivity> semiTargets)> GetProductivities(List<int> productIds, List<long> semiIds)
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


            var semiTarget = new Dictionary<long, LinkDataObjectTargetProductivity>();
            var productTarget = new Dictionary<int, LinkDataObjectTargetProductivity>();

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


                decimal? rate = null;
                int? productId = null;
                if (obj.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {
                    productId = (int?)semiToProduct[obj.ObjectId];
                }
                else
                {
                    productId = (int)obj.ObjectId;
                }

                TargetModel targetProductivityInfo = null;

                if (productId.HasValue
                    && productByIds.TryGetValue(productId.Value, out var productInfo)
                    && targetProductivityInfos.TryGetValue(productInfo.TargetProductivity ?? targetProductivityInfos.FirstOrDefault(t => t.Value.Info.IsDefault).Key, out targetProductivityInfo))
                {
                    if (targetProductivityInfo.Info.WorkLoadTypeId == (int)EnumWorkloadType.Purity)
                    {
                        rate = productInfo.ProductPurity;
                    }

                }

                if (targetProductivityInfo == null)
                {
                    targetProductivityInfo = defaultProductivity;
                }


                if (obj.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {

                    semiTarget.TryAdd(obj.ObjectId, new LinkDataObjectTargetProductivity()
                    {
                        Id = obj.ObjectId,
                        Rate = rate,
                        Target = targetProductivityInfo
                    });
                }
                else
                {
                    productTarget.TryAdd((int)obj.ObjectId, new LinkDataObjectTargetProductivity()
                    {
                        Id = obj.ObjectId,
                        Rate = rate,
                        Target = targetProductivityInfo
                    });
                }

            }
            return (productTarget, semiTarget);

        }

        class LinkDataObjectModel
        {
            public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
            public long ObjectId { get; set; }
        }

        public class TargetModel
        {
            public TargetProductivityEntity Info { get; set; }
            public Dictionary<int, TargetProductivityDetail> BySteps { get; set; }
        }

        public class LinkDataObjectTargetProductivity
        {
            public long Id { get; set; }
            public decimal? Rate { get; set; }
            public TargetModel Target { get; set; }
        }
    }
}
