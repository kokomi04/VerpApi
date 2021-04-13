using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.Stock;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionMaterialsRequirementService : IProductionMaterialsRequirementService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IInventoryRequirementHelperService _inventoryRequirementHelperService;

        public ProductionMaterialsRequirementService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionMaterialsRequirementService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , ICurrentContextService currentContextService
            , IOrganizationHelperService organizationHelperService
            , IInventoryRequirementHelperService inventoryRequirementHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _currentContextService = currentContextService;
            _organizationHelperService = organizationHelperService;
            _inventoryRequirementHelperService = inventoryRequirementHelperService;
        }

        public async Task<long> AddProductionMaterialsRequirement(ProductionMaterialsRequirementModel model, EnumProductionMaterialsRequirementStatus status)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                if (model.MaterialsRequirementDetails.Count == 0)
                    throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundDetailMaterials);
                try
                {
                    if (!model.RequirementDate.HasValue)
                    {
                        model.RequirementDate = DateTime.UtcNow.GetUnix();
                    }

                    CustomGenCodeOutputModel currentConfig = null;
                    if (string.IsNullOrWhiteSpace(model.RequirementCode))
                    {
                        currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.ProductionMaterialsRequirement, EnumObjectType.ProductionMaterialsRequirement, 0, null, model.RequirementCode, model.RequirementDate);
                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                        }
                        bool isFirst = true;
                        do
                        {
                            if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                            var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId,
                                currentConfig.CurrentLastValue.LastValue, null, model.RequirementCode, model.RequirementDate);
                            if (generated == null)
                            {
                                throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                            }
                            model.RequirementCode = generated.CustomCode;
                            isFirst = false;
                        } while (_manufacturingDBContext.ProductionMaterialsRequirement.Any(o => o.RequirementCode == model.RequirementCode));

                    }
                    else
                    {
                        // Validate unique
                        if (_manufacturingDBContext.ProductionMaterialsRequirement.Any(o => o.RequirementCode == model.RequirementCode))
                            throw new BadRequestException(ProductionMaterialsRequirementErrorCode.OutsoureOrderCodeAlreadyExisted);
                    }


                    var requirement = _mapper.Map<ProductionMaterialsRequirement>(model);
                    requirement.CensorStatus = (int)status;

                    _manufacturingDBContext.ProductionMaterialsRequirement.Add(requirement);
                    await _manufacturingDBContext.SaveChangesAsync();

                    foreach (var item in model.MaterialsRequirementDetails)
                    {
                        item.ProductionMaterialsRequirementId = requirement.ProductionMaterialsRequirementId;

                        var entity = _mapper.Map<ProductionMaterialsRequirementDetail>(item);
                        _manufacturingDBContext.ProductionMaterialsRequirementDetail.Add(entity);
                        await _manufacturingDBContext.SaveChangesAsync();

                        requirement.ProductionMaterialsRequirementDetail.Add(entity);
                    }

                    await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                    if (status == EnumProductionMaterialsRequirementStatus.Accepted)
                        await AddInventoryRequirement(model);

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionMaterialsRequirement, requirement.ProductionMaterialsRequirementId, "Thêm mới yêu cầu vật tư thêm", requirement.JsonSerialize());

                    return requirement.ProductionMaterialsRequirementId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("AddProductionMaterialsRequirement");
                    throw ex;
                }
            }
        }

        public async Task<bool> DeleteProductionMaterialsRequirement(long requirementId)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement.FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);

                ValidProductionMaterialsRequirement(requirement);

                var detail = await _manufacturingDBContext.ProductionMaterialsRequirementDetail
                    .Where(x => x.ProductionMaterialsRequirementId == requirementId)
                    .ToListAsync();

                requirement.IsDeleted = true;
                detail.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ProductionMaterialsRequirement, requirement.ProductionMaterialsRequirementId, "Xóa yêu cầu vật tư thêm", requirement.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("DeleteProductionMaterialsRequirement", ex);
                throw;
            }
        }

        private void ValidProductionMaterialsRequirement(ProductionMaterialsRequirement requirement)
        {
            if (requirement == null)
                throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundRequirement);

            if (requirement.CensorStatus == (int)EnumInventoryRequirementStatus.Accepted)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu vật tư thêm đã được duyệt");

            if (requirement.CensorStatus == (int)EnumInventoryRequirementStatus.Rejected)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu vật tư thêm đã bị từ chối");

            if(requirement.ProductionMaterialsRequirementDetail.Any(x=>x.Quantity == 0))
                throw new BadRequestException(GeneralCode.InvalidParams, $"Chi tiết yêu cầu thêm phải có lượng lớn hơn  0");
        }

        public async Task<ProductionMaterialsRequirementModel> GetProductionMaterialsRequirement(long requirementId)
        {
            var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement.AsNoTracking()
                .ProjectTo<ProductionMaterialsRequirementModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);
            if (requirement == null)
                throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundRequirement);

            return requirement;
        }

        public async Task<PageData<ProductionMaterialsRequirementDetailSearch>> SearchProductionMaterialsRequirement(long productionOrderId, string keyword, int page, int size, Clause filters)
        {
            var requirements = (await _manufacturingDBContext.ProductionMaterialsRequirement.AsNoTracking()
               .Where(x => x.ProductionOrderId == productionOrderId)
               .ProjectTo<ProductionMaterialsRequirementModel>(_mapper.ConfigurationProvider)
               .ToListAsync()).SelectMany(r => r.MaterialsRequirementDetails
                    , (p, c) => new ProductionMaterialsRequirementDetailSearch
                    {
                        CensorStatus = p.CensorStatus,
                        CreatedByUserId = p.CreatedByUserId,
                        DepartmentId = c.DepartmentId,
                        ProductId = c.ProductId,
                        ProductionMaterialsRequirementDetailId = c.ProductionMaterialsRequirementDetailId,
                        RequirementCode = p.RequirementCode,
                        ProductionMaterialsRequirementId = p.ProductionMaterialsRequirementId,
                        ProductionStepId = c.ProductionStepId,
                        ProductionStepTitle = c.ProductionStepTitle,
                        RequirementContent = p.RequirementContent,
                        Quantity = c.Quantity,
                        RequirementDate = p.RequirementDate,
                        CreatedDatetimeUtc = p.CreatedDatetimeUtc
                    });

            var productIds = requirements.Select(x => x.ProductId).ToArray();
            var departmentIds = requirements.Select(x => x.DepartmentId).ToArray();

            var products = await _productHelperService.GetListProducts(productIds);
            var departments = await _organizationHelperService.GetDepartmentSimples(departmentIds);

            var query = (from r in requirements
                         join p in products on r.ProductId equals p.ProductId into pi
                         from p in pi.DefaultIfEmpty()
                         join d in departments on r.DepartmentId equals d.DepartmentId into di
                         from d in di.DefaultIfEmpty()
                         select new ProductionMaterialsRequirementDetailSearch
                         {
                             CensorStatus = r.CensorStatus,
                             CreatedByUserId = r.CreatedByUserId,
                             DepartmentId = r.DepartmentId,
                             ProductId = r.ProductId,
                             ProductionMaterialsRequirementDetailId = r.ProductionMaterialsRequirementDetailId,
                             RequirementCode = r.RequirementCode,
                             ProductionMaterialsRequirementId = r.ProductionMaterialsRequirementId,
                             ProductionStepId = r.ProductionStepId,
                             ProductionStepTitle = r.ProductionStepTitle,
                             RequirementContent = r.RequirementContent,
                             Quantity = r.Quantity,
                             RequirementDate = r.RequirementDate,
                             ProductTitle = p != null ? string.Concat(p.ProductCode, "/ ", p.ProductName) : string.Empty,
                             UnitId = p != null ? p.UnitId : 0,
                             DepartmentTitle = d != null ? string.Concat(d.DepartmentCode, "/ ", d.DepartmentName) : string.Empty,
                             CreatedDatetimeUtc = r.CreatedDatetimeUtc
                         }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.ProductTitle.Contains(keyword)
                        || x.RequirementCode.Contains(keyword)
                        || x.ProductionStepTitle.Contains(keyword)
                        || x.DepartmentTitle.Contains(keyword));
            }

            if (filters != null)
            {
                query = query.InternalFilter(filters);
            }

            var lst = (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToList();

            var total = query.Count();

            return (lst, total);
        }

        public async Task<IList<ProductionMaterialsRequirementDetailListModel>> GetProductionMaterialsRequirementByProductionOrder(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionMaterialsRequirementDetail
                .Include(rd => rd.ProductionMaterialsRequirement)
                .Where(rd => rd.ProductionMaterialsRequirement.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionMaterialsRequirementDetailListModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }


        public async Task<bool> UpdateProductionMaterialsRequirement(long requirementId, ProductionMaterialsRequirementModel model)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement.FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);

                ValidProductionMaterialsRequirement(requirement);

                var detail = await _manufacturingDBContext.ProductionMaterialsRequirementDetail
                    .Where(x => x.ProductionMaterialsRequirementId == requirementId)
                    .ToListAsync();

                _mapper.Map(model, requirement);

                var newDetail = model.MaterialsRequirementDetails.AsQueryable()
                    .ProjectTo<ProductionMaterialsRequirementDetail>(_mapper.ConfigurationProvider)
                    .Where(x => !(x.ProductionMaterialsRequirementDetailId > 0));

                foreach (var item in detail)
                {
                    var modify = model.MaterialsRequirementDetails.FirstOrDefault(x => x.ProductionMaterialsRequirementDetailId == item.ProductionMaterialsRequirementDetailId);
                    if (modify == null)
                        item.IsDeleted = true;
                    else _mapper.Map(modify, item);
                }

                _manufacturingDBContext.ProductionMaterialsRequirementDetail.AddRange(newDetail);
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ProductionMaterialsRequirement, requirement.ProductionMaterialsRequirementId, "Cập nhật yêu cầu vật tư thêm", requirement.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError("UpdateProductionMaterialsRequirement", ex);
                throw;
            }
        }

        public async Task<bool> ConfirmInventoryRequirement(long requirementId, EnumProductionMaterialsRequirementStatus status)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement
                    .Include(x => x.ProductionMaterialsRequirementDetail)
                    .Include(x => x.ProductionOrder)
                    .FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);

                ValidProductionMaterialsRequirement(requirement);

                requirement.CensorStatus = (int)status;
                requirement.CensorByUserId = _currentContextService.UserId;
                requirement.CensorDatetimeUtc = DateTime.UtcNow;
                await _manufacturingDBContext.SaveChangesAsync();

                if(status == EnumProductionMaterialsRequirementStatus.Accepted)
                    await AddInventoryRequirement(_mapper.Map<ProductionMaterialsRequirementModel>(requirement));

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "ConfirmInventoryRequirement");
                throw;
            }

        }

        private async Task AddInventoryRequirement(ProductionMaterialsRequirementModel requirement)
        {
            
            var inventoryRequirementModel = new InventoryRequirementSimpleModel
            {
                ProductionOrderId = requirement.ProductionOrderId,
                InventoryRequirementTypeId = EnumInventoryRequirementType.Additional,
                InventoryOutsideMappingTypeId = EnumInventoryOutsideMappingType.ProductionOrder,
                Date = DateTime.Now.Date.GetUnixUtc(_currentContextService.TimeZoneOffset),
                Content = requirement.RequirementContent,

                InventoryRequirementDetail = requirement.MaterialsRequirementDetails.Select(x => new InventoryRequirementSimpleDetailModel
                {
                    DepartmentId = x.DepartmentId,
                    ProductId = x.ProductId,
                    PrimaryQuantity = x.Quantity,
                    ProductionStepId = x.ProductionStepId,
                    ProductionOrderCode = requirement.ProductionOrderCode
                }).ToList()
            };

            await _inventoryRequirementHelperService.AddInventoryRequirement(EnumInventoryType.Output, inventoryRequirementModel);
        }
    }
}
