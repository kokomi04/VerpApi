using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProcessService : IProductionProcessService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;

        public ProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper
            , IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
        }

        public async Task<long> CreateProductionStep(ProductionStepInfo req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var step = _mapper.Map<ProductionStep>((ProductionStepModel)req);
                    await _manufacturingDBContext.ProductionStep.AddAsync(step);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var rInOutStepLinks = await InsertAndUpdateProductionStepLinkData(step.ProductionStepId, req.ProductionStepLinkDatas);
                    await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, step.ProductionStepId,
                        $"Tạo mới công đoạn {req.ProductionStepId} của {req.ContainerTypeId.GetEnumDescription()} {req.ContainerId}", req.JsonSerialize());
                    return step.ProductionStepId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "CreateProductionStep");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteProductionStepById(long productionStepId)
        {
            var productionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .Include(x => x.ProductionStepLinkDataRole)
                                   .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundProductionStep);

            var productInSteps = await _manufacturingDBContext.ProductionStepLinkData
                .Where(x => productionStep.ProductionStepLinkDataRole.Select(r => r.ProductionStepLinkDataId)
                .Contains(x.ProductionStepLinkDataId)).ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    productionStep.IsDeleted = true;
                    productInSteps.ForEach(s => s.IsDeleted = true);

                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, productionStep.ProductionStepId,
                        $"Xóa công đoạn {productionStep.ProductionStepId} của {((EnumProductionProcess.ContainerType)productionStep.ContainerTypeId).GetEnumDescription()} {productionStep.ContainerId}", productionStep.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "DeleteProductionStepById");
                    throw;
                }
            }
        }

        public async Task<ProductionProcessInfo> GetProductionProcessByContainerId(EnumProductionProcess.ContainerType containerTypeId, long containerId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                        .Include(s => s.ProductionStepLinkDataRole)
                                        .ThenInclude(r => r.ProductionStepLinkData)
                                        .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId)
                                        .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                        .ToListAsync();

            var linkGroups = productionSteps
                .SelectMany(s => s.ProductionStepLinkDatas, (s, d) => new
                {
                    s.ProductionStepId,
                    d.ProductionStepLinkDataId,
                    d.ProductionStepLinkDataRoleTypeId
                })
                .GroupBy(l => l.ProductionStepLinkDataId);

            var productionStepLinks = new List<ProductionStepLinkModel>();

            foreach (var linkGroup in linkGroups)
            {
                var fromIds = linkGroup.Where(l => l.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.ProductionStepLinkDataRoleType.Output).Select(l => l.ProductionStepId).Distinct().ToList();
                var toIds = linkGroup.Where(l => l.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.ProductionStepLinkDataRoleType.Input).Select(l => l.ProductionStepId).Distinct().ToList();
                foreach (var fromId in fromIds)
                {
                    bool bExisted = productionStepLinks.Any(l => l.FromStepId == fromId);
                    foreach (var toId in toIds)
                    {
                        if (!bExisted || !productionStepLinks.Any(l => l.FromStepId == fromId && l.ToStepId == toId))
                        {
                            productionStepLinks.Add(new ProductionStepLinkModel
                            {
                                FromStepId = fromId,
                                ToStepId = toId
                            });
                        }
                    }
                }
            }

            return new ProductionProcessInfo
            {
                ProductionSteps = productionSteps,
                ProductionStepLinks = productionStepLinks
            };
        }

        public async Task<bool> CreateProductionProcess(int productionOrderId)
        {
            // Kiểm tra đã tồn tại quy trình sx gắn với lệnh sx 
            if (_manufacturingDBContext.ProductionStep
                .Any(s => s.ContainerTypeId == (int)EnumProductionProcess.ContainerType.LSX && s.ContainerId == productionOrderId))
                throw new BadRequestException(GeneralCode.InvalidParams, "Quy trình cho lệnh sản xuất đã tồn tại.");

            var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                .Where(o => o.ProductionOrderId == productionOrderId)
                .Select(od => new { od.ProductId, TotalQuantity = od.Quantity + od.ReserveQuantity, od.ProductionOrderDetailId })
                .ToList();
            if (productionOrderDetails.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại sản phẩm trong lệnh sản xuất.");

            var productIds = productionOrderDetails.Select(od => od.ProductId).ToList();

            var productOrderMap = productionOrderDetails.ToDictionary(p => (long)p.ProductId, p => p.ProductionOrderDetailId);

            var products = await _productHelperService.GetListProducts(productIds);
            if (productIds.Count > products.Count) throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng không tồn tại.");

            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerTypeId == (int)EnumProductionProcess.ContainerType.SP && productIds.Contains(s.ContainerId))
                .ToList();

            var productionStepIds = productionSteps.Select(s => s.ProductionStepId).ToList();

            var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(r => productionStepIds.Contains(r.ProductionStepId))
                .ToList();
            var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).ToList();
            var linkDatas = _manufacturingDBContext.ProductionStepLinkData.Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    // Tạo step ứng với quy trình sản xuất
                    var productMap = new Dictionary<long, ProductionStep>();
                    foreach (var product in products)
                    {
                        var newStep = new ProductionStep
                        {
                            StepId = null,
                            Title = $"{product.ProductCode} / {product.ProductName}", // Thiếu tên sản phẩm
                            ContainerTypeId = (int)EnumProductionProcess.ContainerType.LSX,
                            ContainerId = productionOrderId,
                            IsGroup = true
                        };
                        _manufacturingDBContext.ProductionStep.Add(newStep);
                        productMap.Add(product.ProductId.Value, newStep);
                    }
                    _manufacturingDBContext.SaveChanges();

                    // Map step quy trình với chi tiết lệnh sản xuất
                    foreach (var item in productMap)
                    {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                        {
                            ProductionOrderDetailId = productOrderMap[item.Key],
                            ProductionStepId = item.Value.ProductionStepId
                        });
                    }

                    // create productionStep
                    var stepMap = new Dictionary<long, ProductionStep>();
                    var stepOrderMap = new Dictionary<long, ProductionStep>();

                    var parentIdUpdater = new List<ProductionStep>();
                    foreach (var step in productionSteps)
                    {
                        var newStep = new ProductionStep
                        {
                            StepId = step.StepId,
                            Title = step.Title,
                            ContainerTypeId = (int)EnumProductionProcess.ContainerType.LSX,
                            ContainerId = productionOrderId,
                            IsGroup = productionSteps.Any(s => s.ParentId == step.ProductionStepId)
                        };
                        if (step.ParentId.HasValue)
                        {
                            parentIdUpdater.Add(step);
                        }
                        else
                        {
                            newStep.ParentId = productMap[step.ContainerId].ProductionStepId;
                        }
                        _manufacturingDBContext.ProductionStep.Add(newStep);
                        stepMap.Add(step.ProductionStepId, newStep);
                        stepOrderMap.Add(step.ContainerId, newStep);

                    }
                    _manufacturingDBContext.SaveChanges();

                    // update parentId
                    foreach (var step in parentIdUpdater)
                    {
                        if (!step.ParentId.HasValue) continue;
                        stepMap[step.ProductionStepId].ParentId = stepMap[step.ParentId.Value].ProductionStepId;
                    }

                    // Map step công đoạn, quy trình con với chi tiết lệnh sản xuất
                    foreach (var item in stepOrderMap)
                    {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                        {
                            ProductionOrderDetailId = productOrderMap[item.Key],
                            ProductionStepId = item.Value.ProductionStepId
                        });
                    }

                    // Create data
                    var linkDataMap = new Dictionary<long, ProductionStepLinkData>();
                    foreach (var item in linkDatas)
                    {
                        var linkDataRole = linkDataRoles.First(r => r.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                        var newLinkData = new ProductionStepLinkData
                        {
                            ObjectId = item.ObjectId,
                            ObjectTypeId = item.ObjectTypeId,
                            UnitId = item.UnitId,
                            Quantity = item.Quantity,
                            SortOrder = item.SortOrder
                        };

                        _manufacturingDBContext.ProductionStepLinkData.Add(newLinkData);
                        linkDataMap.Add(item.ProductionStepLinkDataId, newLinkData);
                    }
                    _manufacturingDBContext.SaveChanges();

                    // Create role
                    foreach (var role in linkDataRoles)
                    {
                        var newRole = new ProductionStepLinkDataRole
                        {
                            ProductionStepLinkDataId = linkDataMap[role.ProductionStepLinkDataId].ProductionStepLinkDataId,
                            ProductionStepId = stepMap[role.ProductionStepId].ProductionStepId,
                            ProductionStepLinkDataRoleTypeId = role.ProductionStepLinkDataRoleTypeId
                        };
                        _manufacturingDBContext.ProductionStepLinkDataRole.Add(newRole);
                    }
                    _manufacturingDBContext.SaveChanges();
                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "CreateProductionProcess");
                    throw;
                }
            }
        }

        public async Task<bool> MergeProductionProcess(int productionOrderId, IList<long> productionStepIds)
        {
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerId == productionOrderId
                && productionStepIds.Contains(s.ProductionStepId)
                && s.ContainerTypeId == (int)EnumProductionProcess.ContainerType.LSX
                && !s.ParentId.HasValue
                && !s.StepId.HasValue)
                .ToList();

            if (productionSteps.Count != productionStepIds.Count()) throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy quy trình trong lệnh sản xuất");

            var productionStepOrder = _manufacturingDBContext.ProductionStepOrder.Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    // Tạo mới
                    var productionStep = new ProductionStep
                    {
                        Title = $"Quy trình chung ({string.Join(",", productionSteps.Select(s => s.Title).ToList())}",
                        ContainerTypeId = (int)EnumProductionProcess.ContainerType.LSX,
                        ContainerId = productionOrderId,
                        IsGroup = true
                    };
                    _manufacturingDBContext.ProductionStep.Add(productionStep);

                    // Xóa danh sách cũ
                    foreach (var item in productionSteps)
                    {
                        item.IsDeleted = true;
                    }
                    _manufacturingDBContext.SaveChanges();

                    // Maping quy trình với chi tiết lệnh SX
                    foreach (var item in productionStepOrder)
                    {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                        {
                            ProductionStepId = productionStep.ProductionStepId,
                            ProductionOrderDetailId = item.ProductionOrderDetailId
                        });
                    }
                    _manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);

                    var childProductionSteps = _manufacturingDBContext.ProductionStep.Where(s => productionStepIds.Contains(s.ParentId)).ToList();
                    // Cập nhật parentStep
                    foreach (var child in childProductionSteps)
                    {
                        child.ParentId = productionStep.ProductionStepId;
                    }

                    _manufacturingDBContext.SaveChanges();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "MergeProductionProcess");
                    throw;
                }
            }
        }

        public async Task<bool> MergeProductionStep(int productionOrderId, IList<long> productionStepIds)
        {
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerId == productionOrderId && productionStepIds.Contains(s.ProductionStepId) && s.ContainerTypeId == (int)EnumProductionProcess.ContainerType.LSX)
                .ToList();
            if (productionSteps.Count <= 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu gộp 2 công đoạn trở lên");

            if (productionSteps.Count != productionStepIds.Count())
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy quy trình trong lệnh sản xuất");

            if (productionSteps.Any(s => !s.StepId.HasValue))
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được phép gộp quy trình con");

            var roles = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(r => productionStepIds.Contains(r.ProductionStepId))
                .ToList();

            var linkDataIds = roles.Select(r => r.ProductionStepLinkDataId).Distinct().ToList();

            var linkDatas = _manufacturingDBContext.ProductionStepLinkData
                .Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId))
                .ToDictionary(d => d.ProductionStepLinkDataId, d => d);

            var group = roles.GroupBy(r => r.ProductionStepId).ToDictionary(g => g.Key, g => g.Select(r => new
            {
                r.ProductionStepLinkDataRoleTypeId,
                linkDatas[r.ProductionStepLinkDataId].ObjectId,
                linkDatas[r.ProductionStepLinkDataId].ObjectTypeId,
            }).ToList());

            // Validate loại công đoạn
            if (productionSteps.Select(s => s.StepId).Distinct().Count() != 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được gộp các công đoạn không đồng nhất");

            // validate input, output
            var firstStepId = productionStepIds[0];
            for (int indx = 1; indx < productionStepIds.Count; indx++)
            {
                var stepId = productionStepIds[indx];
                var bOk = group[stepId].Count == group[firstStepId].Count
                    && group[stepId].All(r => group[firstStepId].Any(p => p.ObjectId == r.ObjectId && p.ObjectTypeId == r.ObjectTypeId && p.ProductionStepLinkDataRoleTypeId == r.ProductionStepLinkDataRoleTypeId));
                if (!bOk) throw new BadRequestException(GeneralCode.InvalidParams, "Không được gộp các công đoạn không đồng nhất");
            }

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    // Gộp các đầu ra, đầu vào step vào step đầu tiên
                    foreach (var role in roles)
                    {
                        if (role.ProductionStepId == productionStepIds[0]) continue;
                        role.ProductionStepId = productionStepIds[0];
                    }
                    // Xóa các step còn lại
                    foreach (var step in productionSteps)
                    {
                        if (step.ProductionStepId == productionStepIds[0]) continue;
                        step.IsDeleted = true;
                    }

                    // Maping quy trình với chi tiết lệnh SX
                    var productionStepOrder = _manufacturingDBContext.ProductionStepOrder
                        .Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();
                    var orderDetailIds = productionStepOrder.Select(so => so.ProductionOrderDetailId).Distinct();
                    // Xóa mapping cũ 
                    _manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);
                    // Tạo lại mapping mới
                    foreach (var orderDetailId in orderDetailIds)
                    {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                        {
                            ProductionStepId = productionStepIds[0],
                            ProductionOrderDetailId = orderDetailId
                        });
                    }

                    _manufacturingDBContext.SaveChanges();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "MergeProductionStep");
                    throw;
                }
            }
        }


        public async Task<ProductionStepInfo> GetProductionStepById(long productionStepId)
        {
            var productionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                    .Where(s => s.ProductionStepId == productionStepId)
                                    .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundProductionStep);

            productionStep.ProductionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                                            .Where(d => d.ProductionStepId == productionStep.ProductionStepId)
                                            .Include(x => x.ProductionStep)
                                            .ProjectTo<ProductionStepLinkDataInfo>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return productionStep;
        }

        public async Task<bool> UpdateProductionStepById(long productionStepId, ProductionStepInfo req)
        {
            var sProductionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .FirstOrDefaultAsync();
            if (sProductionStep == null)
                throw new BadRequestException(ProductionStagesErrorCode.NotFoundProductionStep);

            var dInOutStepLinks = await _manufacturingDBContext.ProductionStepLinkDataRole
                                    .Where(x => x.ProductionStepId == sProductionStep.ProductionStepId)
                                    .ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map((ProductionStepModel)req, sProductionStep);

                    var rInOutStepLinks = await InsertAndUpdateProductionStepLinkData(sProductionStep.ProductionStepId, req.ProductionStepLinkDatas);
                    _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(dInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, sProductionStep.ProductionStepId,
                        $"Cập nhật công đoạn {sProductionStep.ProductionStepId} của {((EnumProductionProcess.ContainerType)sProductionStep.ContainerTypeId).GetEnumDescription()} {sProductionStep.ContainerId}", req.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductionStepById");
                    throw;
                }
            }

        }

        private async Task<IList<ProductionStepLinkDataRole>> InsertAndUpdateProductionStepLinkData(long productionStepId, List<ProductionStepLinkDataInfo> source)
        {
            var nProductionInSteps = source.Where(x => x.ProductionStepLinkDataId <= 0)
                                            .Select(x => _mapper.Map<ProductionStepLinkData>((ProductionStepLinkDataModel)x)).ToList();

            var uProductionInSteps = source.Where(x => x.ProductionStepLinkDataId > 0)
                                            .Select(x => (ProductionStepLinkDataModel)x).ToList();

            var destProductionInSteps = _manufacturingDBContext.ProductionStepLinkData.AsEnumerable()
                .Where(x => uProductionInSteps.Any(y => x.ObjectId == y.ObjectId && x.ObjectTypeId == (int)y.ObjectTypeId)).ToList();

            foreach (var d in destProductionInSteps)
            {
                var s = uProductionInSteps.FirstOrDefault(s => s.ObjectId == d.ObjectId && (int)s.ObjectTypeId == d.ObjectTypeId);
                _mapper.Map(s, d);
            }

            await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(nProductionInSteps);
            await _manufacturingDBContext.SaveChangesAsync();

            var inOutStepLinks = source.Where(x => x.ProductionStepLinkDataId > 0).Select(x => new ProductionStepLinkDataRole
            {
                ProductionStepLinkDataRoleTypeId = (int)x.ProductionStepLinkDataRoleTypeId,
                ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                ProductionStepId = productionStepId
            }).ToList();

            foreach (var p in nProductionInSteps)
            {
                var s = source.FirstOrDefault(x => x.ObjectId == p.ObjectId && (int)x.ObjectTypeId == p.ObjectTypeId);
                if (s == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                inOutStepLinks.Add(new ProductionStepLinkDataRole
                {
                    ProductionStepLinkDataRoleTypeId = (int)s.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataId = p.ProductionStepLinkDataId,
                    ProductionStepId = productionStepId
                });
            }

            return inOutStepLinks;
        }

        public async Task<bool> InsertAndUpdatePorductionStepRoleClient(ProductionStepRoleClientModel  model)
        {
            var info = _manufacturingDBContext.ProductionStepRoleClient.Where(x => x.ContainerId == model.ContainerId && x.ContainerTypeId == model.ContainerTypeId).FirstOrDefault();
            if(info != null)
                info.ClientData = model.ClientData;
            else
                _manufacturingDBContext.Add(_mapper.Map<ProductionStepRoleClient>(model));

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetPorductionStepRoleClient(int containerTypeId, long containerId)
        {
            var info = _manufacturingDBContext.ProductionStepRoleClient.Where(x => x.ContainerId == containerId && x.ContainerTypeId == containerTypeId).FirstOrDefault();
            if (info == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            return info.ClientData;
        }
    }
}
