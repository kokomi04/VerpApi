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
    public class ProductionMaterialsRequirementService: IProductionMaterialsRequirementService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICurrentContextService _currentContextService;

        public ProductionMaterialsRequirementService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionMaterialsRequirementService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , ICurrentContextService currentContextService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _currentContextService = currentContextService;
        }

        public async Task<long> AddProductionMaterialsRequirement(ProductionMaterialsRequirementModel model)
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
                    requirement.CensorStatus = (int)EnumProductionMaterialsRequirementStatus.Waiting;

                    _manufacturingDBContext.ProductionMaterialsRequirement.Add(requirement);
                    await _manufacturingDBContext.SaveChangesAsync();

                    foreach( var item in model.MaterialsRequirementDetails)
                    {
                        item.ProductionMaterialsRequirementId = requirement.ProductionMaterialsRequirementId;

                        var entity = _mapper.Map<ProductionMaterialsRequirementDetail>(item);
                        _manufacturingDBContext.ProductionMaterialsRequirementDetail.Add(entity);
                    }
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);
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
                if (requirement == null)
                    throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundRequirement);

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

        public async Task<ProductionMaterialsRequirementModel> GetProductionMaterialsRequirement(long requirementId)
        {
            var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement.AsNoTracking()
                .ProjectTo<ProductionMaterialsRequirementModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);
            if (requirement == null)
                throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundRequirement);

            return requirement;
        }

        public async Task<PageData<ProductionMaterialsRequirementDetailSearch>> SearchProductionMaterialsRequirement(string keyword, int page, int size, Clause filters)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.RequirementCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductionStepTitle LIKE @KeyWord )");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }

            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vProductionMaterialsRequirementDeailExtractInfo", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vProductionMaterialsRequirementDeailExtractInfo v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.ProductionMaterialsRequirementDetailId) Total FROM vProductionMaterialsRequirementDeailExtractInfo v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.ProductionMaterialsRequirementId");

            var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (size >= 0)
            {
                sql.Append(@$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY");
            }

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionMaterialsRequirementDetailExtrackInfo>().AsQueryable()
                .ProjectTo<ProductionMaterialsRequirementDetailSearch>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<bool> UpdateProductionMaterialsRequirement(long requirementId, ProductionMaterialsRequirementModel model)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement.FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);
                if (requirement == null)
                    throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundRequirement);

                var detail = await _manufacturingDBContext.ProductionMaterialsRequirementDetail
                    .Where(x => x.ProductionMaterialsRequirementId == requirementId)
                    .ToListAsync();

                _mapper.Map(model, requirement);

                var newDetail = model.MaterialsRequirementDetails.AsQueryable()
                    .ProjectTo<ProductionMaterialsRequirementDetail>(_mapper.ConfigurationProvider)
                    .Where(x => !(x.ProductionMaterialsRequirementDetailId > 0));

                foreach(var item in detail)
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
            var requirement = await _manufacturingDBContext.ProductionMaterialsRequirement.FirstOrDefaultAsync(x => x.ProductionMaterialsRequirementId == requirementId);
            if (requirement == null)
                throw new BadRequestException(ProductionMaterialsRequirementErrorCode.NotFoundRequirement);

            requirement.CensorStatus = (int)status;
            requirement.CensorByUserId = _currentContextService.UserId;
            requirement.CensorDatetimeUtc = DateTime.UtcNow;
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
    }
}
