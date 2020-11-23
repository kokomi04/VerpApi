﻿using AutoMapper;
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
using Microsoft.Data.SqlClient;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement {
    public class ProductionProcessService : IProductionProcessService {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;

        public ProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper
            , IProductHelperService productHelperService) {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
        }

        public async Task<long> CreateProductionStep(ProductionStepInfo req) {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction()) {
                try {
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
                } catch (Exception ex) {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "CreateProductionStep");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteProductionStepById(long productionStepId) {
            var productionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .Include(x => x.ProductionStepLinkDataRole)
                                   .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionStep);

            var productStepLinks = await _manufacturingDBContext.ProductionStepLinkData.Include(x => x.ProductionStepLinkDataRole)
                .Where(x => productionStep.ProductionStepLinkDataRole.Select(r => r.ProductionStepLinkDataId)
                .Contains(x.ProductionStepLinkDataId)).ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction()) {
                try {
                    productionStep.IsDeleted = true;
                    foreach (var p in productStepLinks) {
                        if (p.ProductionStepLinkDataRole.Count > 1)
                            throw new BadRequestException(ProductionProcessErrorCode.InvalidDeleteProductionStep,
                                    "Không thể xóa công đoạn!. Đang tồn tại mối quan hệ với công đoạn khác");
                        p.IsDeleted = true;
                    }

                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, productionStep.ProductionStepId,
                        $"Xóa công đoạn {productionStep.ProductionStepId} của {((EnumProductionProcess.EnumContainerType)productionStep.ContainerTypeId).GetEnumDescription()} {productionStep.ContainerId}", productionStep.JsonSerialize());
                    return true;
                } catch (Exception ex) {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "DeleteProductionStepById");
                    throw;
                }
            }
        }

        public async Task<ProductionProcessInfo> GetProductionProcessByScheduleTurn(long scheduleTurnId) {
            var productOrderDetailIds = _manufacturingDBContext.ProductionSchedule
                .Where(s => s.ScheduleTurnId == scheduleTurnId)
                .Select(s => s.ProductionOrderDetailId)
                .ToList();

            var productionStepIds = _manufacturingDBContext.ProductionStepOrder
                .Where(so => productOrderDetailIds.Contains(so.ProductionOrderDetailId))
                .Select(so => so.ProductionStepId)
                .Distinct()
                .ToList();

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => productionStepIds.Contains(s.ProductionStepId))
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new ProductionProcessInfo {
                ProductionSteps = productionSteps,
                ProductionStepLinks = CalcProductionStepLink(productionSteps)
            };
        }

        public async Task<ProductionProcessModel> GetProductionProcessByContainerId(EnumContainerType containerTypeId, long containerId) {

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId)
                .ToListAsync();


            //Lấy thông tin công đoạn
            var stepInfos = _mapper.Map<List<ProductionStepModel>>(productionSteps);
            //Lấy role chi tiết trong công đoạn
            var roles = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleInput {
                ProductionStepId = s.ProductionStepId,
                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                ProductionStepCode = s.ProductionStepCode,
                ProductionStepLinkDataCode = d.ProductionStepLinkData.ProductionStepLinkDataCode,
                ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
            }).ToList();

            //Lấy thông tin dữ liệu của steplinkdata
            var lsProductionStepId = roles.Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();

            if (lsProductionStepId.Count > 0) whereCondition.Append("v.ProductionStepLinkDataId IN ( ");
            for (int i = 0; i < lsProductionStepId.Count; i++) {
                var number = lsProductionStepId[i];
                string pName = $"@ProductionStepLinkDataId{i + 1}";

                if (i == lsProductionStepId.Count - 1)
                    whereCondition.Append($"{pName} )");
                else
                    whereCondition.Append($"{pName}, ");

                parammeters.Add(new SqlParameter(pName, number));
            }

            if (whereCondition.Length > 0) {
                sql.Append(" WHERE ");
                sql.Append(whereCondition);
            }

            var stepLinkDatas = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                    .ConvertData<ProductionStepLinkDataInput>();

            //Tính toán mối quan hệ giữa (stepLink) các công đoạn
            var linkGroups = roles
               .GroupBy(l => l.ProductionStepLinkDataId);

            var productionStepLinks = new List<ProductionStepLinkModel>();

            foreach (var linkGroup in linkGroups) {

                var fromIds = linkGroup.Where(l => l.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output)
                    .Select(l => new { l.ProductionStepId, l.ProductionStepCode }).Distinct().ToList();
                var toIds = linkGroup.Where(l => l.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input)
                    .Select(l => new { l.ProductionStepId, l.ProductionStepCode }).Distinct().ToList();
                
                foreach (var fromId in fromIds) {
                    bool bExisted = productionStepLinks.Any(l => l.FromStepId == fromId.ProductionStepId);
                    foreach (var toId in toIds) {
                        if (!bExisted || !productionStepLinks.Any(l => l.FromStepId == fromId.ProductionStepId && l.ToStepId == toId.ProductionStepId)) {
                            productionStepLinks.Add(new ProductionStepLinkModel {
                                FromStepCode = fromId.ProductionStepCode,
                                FromStepId = fromId.ProductionStepId,
                                ToStepId = toId.ProductionStepId,
                                ToStepCode = toId.ProductionStepCode
                            });
                        }
                    }
                }
            }

            // Tính toán quan hệ của quy trình con
            var productionStepGroupLinkDataRoles = new List<ProductionStepLinkDataRoleInput>();
            var stepParent = stepInfos.Where(x => x.IsGroup.Value).ToList();
            foreach (var parent in stepParent) {
                // danh sách id công đoạn trong quy trình con, chúng được sắp xếp theo sortOrder
                var lsChildProductionStepId = stepInfos.Where(x => x.ParentId.HasValue && x.ParentId.Value == parent.ProductionStepId)
                    .OrderBy(x => x.SortOrder)
                    .Select(x => x.ProductionStepId)
                    .ToList();

                foreach (var childProductionStepId in lsChildProductionStepId) {
                    /*
                     * Lấy StepLink phía sau công đoạn được kết nối với công đoạn con trong quy trình con. 
                     * Thay đổi giá trị sang quy trình con
                    */
                    var tempToLink = productionStepLinks.Where(x => x.FromStepId == childProductionStepId && !lsChildProductionStepId.Contains(x.ToStepId))
                        .Select(x => new ProductionStepLinkModel {
                            FromStepCode = parent.ProductionStepCode,
                            FromStepId = parent.ProductionStepId,
                            ToStepId = x.ToStepId,
                            ToStepCode = x.ToStepCode
                        }).ToList();
                    /*
                     * Lấy StepLinkDataRole Output của quy trình con
                     */
                    var outputStepLinkDataRoleParent = (from c in roles
                                                        join t in tempToLink
                                                        on c.ProductionStepId equals t.ToStepId
                                                        where c.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                                                        select new ProductionStepLinkDataRoleInput {
                                                            ProductionStepId = parent.ProductionStepId,
                                                            ProductionStepLinkDataId = c.ProductionStepLinkDataId,
                                                            ProductionStepCode = parent.ProductionStepCode,
                                                            ProductionStepLinkDataCode = c.ProductionStepLinkDataCode,
                                                            ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Output,
                                                        }).ToList();

                    /*
                     * Lấy StepLink phía trước công đoạn được kết nối với công đoạn con trong quy trình con.
                     * Thay đổi giá trị sang quy trình con
                    */
                    var tempFromLink = productionStepLinks.Where(x => x.ToStepId == childProductionStepId && !lsChildProductionStepId.Contains(x.FromStepId))
                        .Select(x => new ProductionStepLinkModel {
                            FromStepCode = x.FromStepCode,
                            FromStepId = x.FromStepId,
                            ToStepId = parent.ProductionStepId,
                            ToStepCode = parent.ProductionStepCode
                        }).ToList();

                    /*
                     * Lấy StepLinkDataRole Iutput của quy trình con
                     */
                    var inputStepLinkDataRoleParent = (from c in roles
                                                       join t in tempFromLink
                                                       on c.ProductionStepId equals t.FromStepId
                                                       where c.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                                                       select new ProductionStepLinkDataRoleInput {
                                                           ProductionStepId = parent.ProductionStepId,
                                                           ProductionStepLinkDataId = c.ProductionStepLinkDataId,
                                                           ProductionStepCode = parent.ProductionStepCode,
                                                           ProductionStepLinkDataCode = c.ProductionStepLinkDataCode,
                                                           ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Input,
                                                       }).ToList();
                    // Thêm vào mảng
                    productionStepLinks.AddRange(tempFromLink);
                    productionStepLinks.AddRange(tempToLink);

                    productionStepGroupLinkDataRoles.AddRange(inputStepLinkDataRoleParent);
                    productionStepGroupLinkDataRoles.AddRange(outputStepLinkDataRoleParent);
                }

                /*
                * 1. Lấy id công đoạn đầu và cuối trong quy trình con
                * 2. Lấy input của công đoạn đầu cho quy trình con
                * 3. Lấy output của công đoạn cuối cho quy trình con
                */
                var firstStepId = lsChildProductionStepId.First();
                var lastStepId = lsChildProductionStepId.Last();
                
                var inputFirstStepLinkDataRoleParent = roles.Where(x => x.ProductionStepId == firstStepId 
                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input 
                        && !productionStepGroupLinkDataRoles.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                    .Select(x => new ProductionStepLinkDataRoleInput {
                    ProductionStepId = parent.ProductionStepId,
                    ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                    ProductionStepCode = parent.ProductionStepCode,
                    ProductionStepLinkDataCode = x.ProductionStepLinkDataCode,
                    ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Input,
                    })
                    .ToList();

                var outputLastStepLinkDataRoleParent = roles.Where(x => x.ProductionStepId == lastStepId 
                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output 
                        && !productionStepGroupLinkDataRoles.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                    .Select(x => new ProductionStepLinkDataRoleInput {
                    ProductionStepId = parent.ProductionStepId,
                    ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                    ProductionStepCode = parent.ProductionStepCode,
                    ProductionStepLinkDataCode = x.ProductionStepLinkDataCode,
                    ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Output,
                    })
                    .ToList();

                productionStepGroupLinkDataRoles.AddRange(inputFirstStepLinkDataRoleParent);
                productionStepGroupLinkDataRoles.AddRange(outputLastStepLinkDataRoleParent);
            }

            return new ProductionProcessModel {
                ContainerId = containerId,
                ContainerTypeId = containerTypeId,
                ProductionSteps = stepInfos,
                ProductionStepLinkDataRoles = roles,
                ProductionStepLinkDatas = stepLinkDatas,
                ProductionStepLinks = productionStepLinks,
                ProductionStepGroupLinkDataRoles = productionStepGroupLinkDataRoles
            };
        }

        private List<ProductionStepLinkModel> CalcProductionStepLink(List<ProductionStepInfo> productionSteps) {
            var linkGroups = productionSteps
               .SelectMany(s => s.ProductionStepLinkDatas, (s, d) => new {
                   s.ProductionStepId,
                   d.ProductionStepLinkDataId,
                   d.ProductionStepLinkDataRoleTypeId
               })
               .GroupBy(l => l.ProductionStepLinkDataId);

            var productionStepLinks = new List<ProductionStepLinkModel>();

            foreach (var linkGroup in linkGroups) {
                var fromIds = linkGroup.Where(l => l.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output).Select(l => l.ProductionStepId).Distinct().ToList();
                var toIds = linkGroup.Where(l => l.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).Select(l => l.ProductionStepId).Distinct().ToList();
                foreach (var fromId in fromIds) {
                    bool bExisted = productionStepLinks.Any(l => l.FromStepId == fromId);
                    foreach (var toId in toIds) {
                        if (!bExisted || !productionStepLinks.Any(l => l.FromStepId == fromId && l.ToStepId == toId)) {
                            productionStepLinks.Add(new ProductionStepLinkModel {
                                FromStepId = fromId,
                                ToStepId = toId
                            });
                        }
                    }
                }
            }

            return productionStepLinks;
        }

        public async Task<bool> IncludeProductionProcess(int productionOrderId) {
            // Kiểm tra đã tồn tại quy trình sx gắn với lệnh sx 
            var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                .Where(o => o.ProductionOrderId == productionOrderId)
                .ToList();

            if (productionOrderDetails.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại sản phẩm trong lệnh sản xuất.");

            var productionOrderDetailIds = productionOrderDetails.Select(od => od.ProductionOrderDetailId).ToList();

            var hasProcessDetailIds = _manufacturingDBContext.ProductionStepOrder
                .Where(so => productionOrderDetailIds.Contains(so.ProductionOrderDetailId))
                .Select(so => so.ProductionOrderDetailId)
                .Distinct()
                .ToList();

            // Nếu đã có đầy đủ quy trình => thông báo lỗi
            if (productionOrderDetailIds.Count == hasProcessDetailIds.Count) {
                throw new BadRequestException(GeneralCode.InvalidParams, "Quy trình cho lệnh sản xuất đã hoàn thiện.");
            }
            // Nếu đã có một phần quy trình => remove phần đã tồn tại trong danh sách detail cần tạo
            else if (hasProcessDetailIds.Count > 0) {
                productionOrderDetails.RemoveAll(od => hasProcessDetailIds.Contains(od.ProductionOrderDetailId));
            }

            var productIds = productionOrderDetails.Select(od => od.ProductId).ToList();

            var productOrderMap = productionOrderDetails.ToDictionary(p => (long)p.ProductId, p => p.ProductionOrderDetailId);

            var products = await _productHelperService.GetListProducts(productIds);
            if (productIds.Count > products.Count) throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng không tồn tại.");

            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerTypeId == (int)EnumProductionProcess.EnumContainerType.ProductionOrder && productIds.Contains(s.ContainerId))
                .ToList();

            var productionStepIds = productionSteps.Select(s => s.ProductionStepId).ToList();

            var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(r => productionStepIds.Contains(r.ProductionStepId))
                .ToList();
            var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).ToList();
            var linkDatas = _manufacturingDBContext.ProductionStepLinkData.Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

            using var trans = _manufacturingDBContext.Database.BeginTransaction();
            try {
                // Update status cho chi tiết LSX
                foreach (var item in productionOrderDetails) {
                    item.Status = (int)EnumProductionStatus.Waiting;
                }

                // Tạo step ứng với quy trình sản xuất
                var productMap = new Dictionary<long, ProductionStep>();
                foreach (var product in products) {
                    var newStep = new ProductionStep {
                        StepId = null,
                        Title = $"{product.ProductCode} / {product.ProductName}", // Thiếu tên sản phẩm
                        ContainerTypeId = (int)EnumProductionProcess.EnumContainerType.ProductionOrder,
                        ContainerId = productionOrderId,
                        IsGroup = true
                    };
                    _manufacturingDBContext.ProductionStep.Add(newStep);
                    productMap.Add(product.ProductId.Value, newStep);
                }
                _manufacturingDBContext.SaveChanges();

                // Map step quy trình với chi tiết lệnh sản xuất
                foreach (var item in productMap) {
                    _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder {
                        ProductionOrderDetailId = productOrderMap[item.Key],
                        ProductionStepId = item.Value.ProductionStepId
                    });
                }

                // create productionStep
                var stepMap = new Dictionary<long, ProductionStep>();
                var stepOrderMap = new Dictionary<long, ProductionStep>();

                var parentIdUpdater = new List<ProductionStep>();
                foreach (var step in productionSteps) {
                    var newStep = new ProductionStep {
                        StepId = step.StepId,
                        Title = step.Title,
                        ContainerTypeId = (int)EnumProductionProcess.EnumContainerType.ProductionOrder,

                        ContainerId = productionOrderId,
                        IsGroup = productionSteps.Any(s => s.ParentId == step.ProductionStepId)
                    };
                    if (step.ParentId.HasValue) {
                        parentIdUpdater.Add(step);
                    } else {
                        newStep.ParentId = productMap[step.ContainerId].ProductionStepId;
                    }
                    _manufacturingDBContext.ProductionStep.Add(newStep);
                    stepMap.Add(step.ProductionStepId, newStep);
                    stepOrderMap.Add(step.ContainerId, newStep);

                }
                _manufacturingDBContext.SaveChanges();

                // update parentId
                foreach (var step in parentIdUpdater) {
                    if (!step.ParentId.HasValue) continue;
                    stepMap[step.ProductionStepId].ParentId = stepMap[step.ParentId.Value].ProductionStepId;
                }

                // Map step công đoạn, quy trình con với chi tiết lệnh sản xuất
                foreach (var item in stepOrderMap) {
                    _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder {
                        ProductionOrderDetailId = productOrderMap[item.Key],
                        ProductionStepId = item.Value.ProductionStepId
                    });
                }

                // Create data
                var linkDataMap = new Dictionary<long, ProductionStepLinkData>();
                foreach (var item in linkDatas) {
                    var linkDataRole = linkDataRoles.First(r => r.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                    var newLinkData = new ProductionStepLinkData {
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
                foreach (var role in linkDataRoles) {
                    var newRole = new ProductionStepLinkDataRole {
                        ProductionStepLinkDataId = linkDataMap[role.ProductionStepLinkDataId].ProductionStepLinkDataId,
                        ProductionStepId = stepMap[role.ProductionStepId].ProductionStepId,
                        ProductionStepLinkDataRoleTypeId = role.ProductionStepLinkDataRoleTypeId
                    };
                    _manufacturingDBContext.ProductionStepLinkDataRole.Add(newRole);
                }
                _manufacturingDBContext.SaveChanges();
                await trans.CommitAsync();

                return true;
            } catch (Exception ex) {
                await trans.RollbackAsync();
                _logger.LogError(ex, "CreateProductionProcess");
                throw;
            }
        }

        public async Task<bool> MergeProductionProcess(int productionOrderId, IList<long> productionStepIds) {
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerId == productionOrderId
                && productionStepIds.Contains(s.ProductionStepId)
                && s.ContainerTypeId == (int)EnumProductionProcess.EnumContainerType.ProductionOrder
                && !s.ParentId.HasValue
                && !s.StepId.HasValue)
                .ToList();

            if (productionSteps.Count != productionStepIds.Count()) throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy quy trình trong lệnh sản xuất");

            var productionStepOrder = _manufacturingDBContext.ProductionStepOrder.Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction()) {
                try {
                    // Tạo mới
                    var productionStep = new ProductionStep {
                        Title = $"Quy trình chung ({string.Join(",", productionSteps.Select(s => s.Title).ToList())}",
                        ContainerTypeId = (int)EnumProductionProcess.EnumContainerType.ProductionOrder,
                        ContainerId = productionOrderId,
                        IsGroup = true
                    };
                    _manufacturingDBContext.ProductionStep.Add(productionStep);

                    // Xóa danh sách cũ
                    foreach (var item in productionSteps) {
                        item.IsDeleted = true;
                    }
                    _manufacturingDBContext.SaveChanges();

                    // Maping quy trình với chi tiết lệnh SX
                    foreach (var item in productionStepOrder) {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder {
                            ProductionStepId = productionStep.ProductionStepId,
                            ProductionOrderDetailId = item.ProductionOrderDetailId
                        });
                    }
                    _manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);

                    var childProductionSteps = _manufacturingDBContext.ProductionStep.Where(s => productionStepIds.Contains(s.ParentId)).ToList();
                    // Cập nhật parentStep
                    foreach (var child in childProductionSteps) {
                        child.ParentId = productionStep.ProductionStepId;
                    }

                    _manufacturingDBContext.SaveChanges();

                    await trans.CommitAsync();

                    return true;
                } catch (Exception ex) {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "MergeProductionProcess");
                    throw;
                }
            }
        }

        public async Task<bool> MergeProductionStep(int productionOrderId, IList<long> productionStepIds) {
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerId == productionOrderId && productionStepIds.Contains(s.ProductionStepId) && s.ContainerTypeId == (int)EnumProductionProcess.EnumContainerType.ProductionOrder)
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

            var group = roles.GroupBy(r => r.ProductionStepId).ToDictionary(g => g.Key, g => g.Select(r => new {
                r.ProductionStepLinkDataRoleTypeId,
                linkDatas[r.ProductionStepLinkDataId].ObjectId,
                linkDatas[r.ProductionStepLinkDataId].ObjectTypeId,
            }).ToList());

            // Validate loại công đoạn
            if (productionSteps.Select(s => s.StepId).Distinct().Count() != 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được gộp các công đoạn không đồng nhất");

            // validate input, output
            var firstStepId = productionStepIds[0];
            for (int indx = 1; indx < productionStepIds.Count; indx++) {
                var stepId = productionStepIds[indx];
                var bOk = group[stepId].Count == group[firstStepId].Count
                    && group[stepId].All(r => group[firstStepId].Any(p => p.ObjectId == r.ObjectId && p.ObjectTypeId == r.ObjectTypeId && p.ProductionStepLinkDataRoleTypeId == r.ProductionStepLinkDataRoleTypeId));
                if (!bOk) throw new BadRequestException(GeneralCode.InvalidParams, "Không được gộp các công đoạn không đồng nhất");
            }

            using (var trans = _manufacturingDBContext.Database.BeginTransaction()) {
                try {
                    // Gộp các đầu ra, đầu vào step vào step đầu tiên
                    foreach (var role in roles) {
                        if (role.ProductionStepId == productionStepIds[0]) continue;
                        role.ProductionStepId = productionStepIds[0];
                    }
                    // Xóa các step còn lại
                    foreach (var step in productionSteps) {
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
                    foreach (var orderDetailId in orderDetailIds) {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder {
                            ProductionStepId = productionStepIds[0],
                            ProductionOrderDetailId = orderDetailId
                        });
                    }

                    _manufacturingDBContext.SaveChanges();

                    await trans.CommitAsync();

                    return true;
                } catch (Exception ex) {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "MergeProductionStep");
                    throw;
                }
            }
        }


        public async Task<ProductionStepInfo> GetProductionStepById(long productionStepId) {
            var productionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                    .Where(s => s.ProductionStepId == productionStepId)
                                    .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionStep);

            productionStep.ProductionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                                            .Where(d => d.ProductionStepId == productionStep.ProductionStepId)
                                            .Include(x => x.ProductionStep)
                                            .ProjectTo<ProductionStepLinkDataInfo>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return productionStep;
        }

        public async Task<bool> UpdateProductionStepById(long productionStepId, ProductionStepInfo req) {
            var sProductionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .FirstOrDefaultAsync();
            if (sProductionStep == null)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionStep);

            var dInOutStepLinks = await _manufacturingDBContext.ProductionStepLinkDataRole
                                    .Where(x => x.ProductionStepId == sProductionStep.ProductionStepId)
                                    .ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction()) {
                try {
                    _mapper.Map((ProductionStepModel)req, sProductionStep);

                    var rInOutStepLinks = await InsertAndUpdateProductionStepLinkData(sProductionStep.ProductionStepId, req.ProductionStepLinkDatas);
                    _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(dInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, sProductionStep.ProductionStepId,
                        $"Cập nhật công đoạn {sProductionStep.ProductionStepId} của {((EnumProductionProcess.EnumContainerType)sProductionStep.ContainerTypeId).GetEnumDescription()} {sProductionStep.ContainerId}", req.JsonSerialize());
                    return true;
                } catch (Exception ex) {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductionStepById");
                    throw;
                }
            }

        }

        private async Task<IList<ProductionStepLinkDataRole>> InsertAndUpdateProductionStepLinkData(long productionStepId, List<ProductionStepLinkDataInfo> source) {
            var nProductionInSteps = source.Where(x => x.ProductionStepLinkDataId <= 0)
                                            .Select(x => _mapper.Map<ProductionStepLinkData>((ProductionStepLinkDataModel)x)).ToList();

            var uProductionInSteps = source.Where(x => x.ProductionStepLinkDataId > 0)
                                            .Select(x => (ProductionStepLinkDataModel)x).ToList();

            var destProductionInSteps = _manufacturingDBContext.ProductionStepLinkData
                .Where(x => uProductionInSteps.Select(x => x.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId)).ToList();

            foreach (var d in destProductionInSteps) {
                var s = uProductionInSteps.FirstOrDefault(s => s.ProductionStepLinkDataId == d.ProductionStepLinkDataId);
                if (s != null)
                    _mapper.Map(s, d);
            }

            await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(nProductionInSteps);
            await _manufacturingDBContext.SaveChangesAsync();

            var inOutStepLinks = source.Where(x => x.ProductionStepLinkDataId > 0).Select(x => new ProductionStepLinkDataRole {
                ProductionStepLinkDataRoleTypeId = (int)x.ProductionStepLinkDataRoleTypeId,
                ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                ProductionStepId = productionStepId
            }).ToList();

            foreach (var p in nProductionInSteps) {
                var s = source.FirstOrDefault(x => x.ObjectId == p.ObjectId && (int)x.ObjectTypeId == p.ObjectTypeId);
                if (s == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                inOutStepLinks.Add(new ProductionStepLinkDataRole {
                    ProductionStepLinkDataRoleTypeId = (int)s.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataId = p.ProductionStepLinkDataId,
                    ProductionStepId = productionStepId
                });
            }

            return inOutStepLinks;
        }

        public async Task<bool> InsertAndUpdatePorductionStepRoleClient(ProductionStepRoleClientModel model) {
            var info = _manufacturingDBContext.ProductionStepRoleClient.Where(x => x.ContainerId == model.ContainerId && x.ContainerTypeId == model.ContainerTypeId).FirstOrDefault();
            if (info != null)
                info.ClientData = model.ClientData;
            else
                _manufacturingDBContext.Add(_mapper.Map<ProductionStepRoleClient>(model));

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetPorductionStepRoleClient(int containerTypeId, long containerId) {
            var info = await _manufacturingDBContext.ProductionStepRoleClient.Where(x => x.ContainerId == containerId && x.ContainerTypeId == containerTypeId).FirstOrDefaultAsync();
            if (info == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            return info.ClientData;
        }

        public async Task<long> CreateProductionStepGroup(ProductionStepGroupModel req) {
            var stepGroup = _mapper.Map<ProductionStep>(req);
            stepGroup.IsGroup = true;
            _manufacturingDBContext.ProductionStep.Add(stepGroup);
            await _manufacturingDBContext.SaveChangesAsync();

            var child = _manufacturingDBContext.ProductionStep.Where(s => req.ListProductionStepId.Contains(s.ProductionStepId));
            foreach (var c in child) {
                c.ParentId = stepGroup.ProductionStepId;
            }
            await _manufacturingDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ProductionStep, stepGroup.ProductionStepId,
                        $"Tạo mới quy trình con {req.ProductionStepId} của {req.ContainerTypeId.GetEnumDescription()} {req.ContainerId}", req.JsonSerialize());
            return stepGroup.ProductionStepId;
        }

        public async Task<bool> UpdateProductionStepSortOrder(IList<PorductionStepSortOrderModel> req) {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try {
                var lsProductionStep = await _manufacturingDBContext.ProductionStep.Where(x => req.Select(y => y.ProductionStepId).Contains(x.ProductionStepId)).ToListAsync();
                foreach (var p in lsProductionStep) {
                    p.SortOrder = req.SingleOrDefault(y => y.ProductionStepId == p.ProductionStepId).SortOrder;
                }
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductionStep, req.First().ProductionStepId,
                        $"Cập nhật vị trí cho các công đoạn", req.JsonSerialize());
                return true;
            } catch (Exception ex) {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateProductionStepSortOrder");
                throw;
            }
        }

        #region Production process
        public async Task<bool> CreateProductionProcess(ProductionProcessModel req) {
            ValidProductionStepLinkData(req);

            var isExists = _manufacturingDBContext.ProductionStep.Any(p => p.ContainerId == req.ContainerId && p.ContainerTypeId == (int)req.ContainerTypeId);
            if (isExists)
                throw new BadRequestException(ProductionProcessErrorCode.ExistsProductionProcess);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try {
                var productionStepModel = _mapper.Map<List<ProductionStep>>(req.ProductionSteps);
                var productionStepLinkData = _mapper.Map<List<ProductionStepLinkData>>(req.ProductionStepLinkDatas);

                await _manufacturingDBContext.ProductionStep.AddRangeAsync(productionStepModel);
                await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(productionStepLinkData);

                //Gán parentId nếu trong nhóm công đoạn có QTSX con
                foreach (var s in productionStepModel) {
                    if (!string.IsNullOrWhiteSpace(s.ParentCode)) {
                        var p = productionStepModel.FirstOrDefault(x => x.ProductionStepCode.Equals(s.ParentCode));
                        if (p != null)
                            s.ParentId = p.ProductionStepId;
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();

                var roles = from r in req.ProductionStepLinkDataRoles
                            join s in productionStepModel on r.ProductionStepCode equals s.ProductionStepCode
                            join d in productionStepLinkData on r.ProductionStepLinkDataCode equals d.ProductionStepLinkDataCode
                            select new ProductionStepLinkDataRole {
                                ProductionStepId = s.ProductionStepId,
                                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                                ProductionStepLinkDataRoleTypeId = (int)r.ProductionStepLinkDataRoleTypeId
                            };
                await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(roles);
                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductionProcess, req.ContainerId, "Tạo quy trình sản xuất", req.JsonSerialize());
                return true;
            } catch (Exception ex) {
                await trans.RollbackAsync();
                _logger.LogError(ex, "CreateProductionProcess");
                throw;
            }
        }

        public async Task<bool> UpdateProductionProcess(EnumContainerType containerTypeId, long containerId, ProductionProcessModel req) {
            ValidProductionStepLinkData(req);
            var isExists = await _manufacturingDBContext.ProductionStep.AnyAsync(p => p.ContainerId == containerId && p.ContainerTypeId == (int)containerTypeId);
            if (!isExists)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionProcess);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try {
                //Cập nhật, xóa và tạo mới step
                var sourceStep = await _manufacturingDBContext.ProductionStep.Where(p => p.ContainerId == containerId && p.ContainerTypeId == (int)containerTypeId).ToListAsync();
                foreach (var dest in sourceStep) {
                    var source = req.ProductionSteps.SingleOrDefault(x => x.ProductionStepId == dest.ProductionStepId);
                    if (source != null)
                        _mapper.Map(source, dest);
                    else dest.IsDeleted = true;
                }

                var newStep = req.ProductionSteps.AsQueryable().ProjectTo<ProductionStep>(_mapper.ConfigurationProvider)
                    .Where(x => !sourceStep.Select(y => y.ProductionStepId).Contains(x.ProductionStepId))
                    .ToList();

                await _manufacturingDBContext.ProductionStep.AddRangeAsync(newStep);
                await _manufacturingDBContext.SaveChangesAsync();

                //Cập nhật, xóa và tạo mới steplinkdata
                var lsStepLinkDataId = (from s in _manufacturingDBContext.ProductionStep
                                        join r in _manufacturingDBContext.ProductionStepLinkDataRole on s.ProductionStepId equals r.ProductionStepId
                                        where s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId
                                        select r.ProductionStepLinkDataId).Distinct();
                var sourceStepLinkData = await _manufacturingDBContext.ProductionStepLinkData.Where(p => lsStepLinkDataId.Contains(p.ProductionStepLinkDataId)).ToListAsync();
                foreach (var dest in sourceStepLinkData) {
                    var source = req.ProductionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == dest.ProductionStepLinkDataId);
                    if (source != null)
                        _mapper.Map(source, dest);
                    else
                        dest.IsDeleted = true;
                }

                var newStepLinkData = req.ProductionStepLinkDatas.AsQueryable().ProjectTo<ProductionStepLinkData>(_mapper.ConfigurationProvider)
                    .Where(x => !sourceStepLinkData.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                    .ToList();

                await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(newStepLinkData);
                await _manufacturingDBContext.SaveChangesAsync();

                //Cập nhật role steplinkdata trong step
                newStep.AddRange(sourceStep.Where(x => !x.IsDeleted).AsEnumerable());
                newStepLinkData.AddRange(sourceStepLinkData.Where(x => !x.IsDeleted).AsEnumerable());

                var roles = from r in req.ProductionStepLinkDataRoles
                            join s in newStep on r.ProductionStepCode equals s.ProductionStepCode
                            join d in newStepLinkData on r.ProductionStepLinkDataCode equals d.ProductionStepLinkDataCode
                            select new ProductionStepLinkDataRole {
                                ProductionStepId = s.ProductionStepId,
                                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                                ProductionStepLinkDataRoleTypeId = (int)r.ProductionStepLinkDataRoleTypeId
                            };
                var oldRoles = _manufacturingDBContext.ProductionStepLinkDataRole.Where(x => newStep.Select(y => y.ProductionStepId).Contains(x.ProductionStepId)).ToList();

                _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(oldRoles);
                await _manufacturingDBContext.SaveChangesAsync();
                await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(roles);

                //Gán parentId nếu trong nhóm công đoạn có QTSX con
                foreach (var s in newStep) {
                    if (!string.IsNullOrWhiteSpace(s.ParentCode)) {
                        var p = newStep.FirstOrDefault(x => x.ProductionStepCode.Equals(s.ParentCode));
                        if (p != null)
                            s.ParentId = p.ProductionStepId;
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductionProcess, req.ContainerId, "Cập nhật quy trình sản xuất", req.JsonSerialize());
                return true;
            } catch (Exception ex) {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateProductionProcess");
                throw;
            }

        }

        #endregion

        private void ValidProductionStepLinkData(ProductionProcessModel req) {
            var lsInputStep = req.ProductionStepLinkDataRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input);
            var lsOutputStep = req.ProductionStepLinkDataRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);

            var distinctLinkData = req.ProductionStepLinkDatas.Select(x => x.ProductionStepLinkDataCode).Distinct();
            foreach (var linkDataCode in distinctLinkData) {
                var inStep = lsInputStep.Where(x => x.ProductionStepLinkDataCode.Equals(linkDataCode)).ToList();
                var outStep = lsOutputStep.Where(x => x.ProductionStepLinkDataCode.Equals(linkDataCode)).ToList();
                if (inStep.Count == 0 && outStep.Count == 0)
                    throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, $"Chi tiết {linkDataCode} không thuộc bất kỳ công đoạn nào");
                if (inStep.Count > 1)
                    throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, $"Chi tiết {linkDataCode} không thể là đầu vào của 2 công đoạn");
                if (outStep.Count > 1)
                    throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, $"Chi tiết {linkDataCode} không thể là đầu ra của 2 công đoạn");
            }
        }
    }
}
