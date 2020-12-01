using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using Microsoft.Data.SqlClient;
using VErp.Services.Stock.Service.Stock;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using System.Linq.Expressions;
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;

namespace VErp.Services.Manafacturing.Service.Stock.Implement
{
    public class InventoryRequirementService : IInventoryRequirementService
    {
        private readonly StockDBContext _stockDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;

        public InventoryRequirementService(StockDBContext stockDBContext
            , IActivityLogService activityLogService
            , ILogger<InventoryRequirementService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService)
        {
            _stockDBContext = stockDBContext;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
        }

        public async Task<PageData<InventoryRequirementModel>> GetListInventoryRequirements(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _stockDBContext.InventoryRequirement.Where(s => s.InventoryTypeId == (int)inventoryType);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s => s.InventoryRequirementCode.Contains(keyword)
                || s.Content.Contains(keyword));
            }
            query = query.InternalFilter(filters);

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
            {
                string command = asc ? "OrderBy" : "OrderByDescending";
                var type = typeof(InventoryRequirement);
                var property = type.GetProperty(orderByFieldName);
                var parameter = Expression.Parameter(type, "s");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);
                var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                              query.Expression, Expression.Quote(orderByExpression));
                query = query.Provider.CreateQuery<InventoryRequirement>(resultExpression);
            }

            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                .ProjectTo<InventoryRequirementModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<InventoryRequirementModel> GetInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId)
        {
            var entity = _stockDBContext.InventoryRequirement
                .Include(r => r.InventoryRequirementDetail)
                .Include(r => r.InventoryRequirementFile)
                .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);
            var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
            if (entity == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");
            return _mapper.Map<InventoryRequirementModel>(entity);
        }

        public async Task<InventoryRequirementModel> AddInventoryRequirement(EnumInventoryType inventoryType, InventoryRequirementModel req)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;
                int customGenCodeId = 0;
                if (string.IsNullOrEmpty(req.InventoryRequirementCode))
                {
                    CustomGenCodeOutputModelOut currentConfig = await _customGenCodeHelperService.CurrentConfig(objectType, objectType, 0);
                    if (currentConfig == null)
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                    }
                    var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
                    if (generated == null)
                    {
                        throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                    }

                    customGenCodeId = currentConfig.CustomGenCodeId;

                    req.InventoryRequirementCode = generated.CustomCode;
                }
                else
                {
                    // Validate unique
                    if (_stockDBContext.InventoryRequirement.Any(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementCode == req.InventoryRequirementCode))
                        throw new BadRequestException(GeneralCode.InternalError, "Mã yêu cầu đã tồn tại");
                }

                var inventoryRequirement = _mapper.Map<InventoryRequirementEntity>(req);
                inventoryRequirement.InventoryTypeId = (int)inventoryType;
                _stockDBContext.InventoryRequirement.Add(inventoryRequirement);
                await _stockDBContext.SaveChangesAsync();

                // Tạo file
                foreach (var item in req.InventoryRequirementFile)
                {
                    // Tạo mới
                    var inventoryRequirementFile = _mapper.Map<InventoryRequirementFile>(item);
                    inventoryRequirementFile.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDBContext.InventoryRequirementFile.Add(inventoryRequirementFile);
                }


                // Tạo detail
                foreach (var item in req.InventoryRequirementDetail)
                {
                    // Tạo mới
                    var inventoryRequirementDetail = _mapper.Map<InventoryRequirementDetail>(item);
                    inventoryRequirementDetail.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDBContext.InventoryRequirementDetail.Add(inventoryRequirementDetail);
                }
                await _stockDBContext.SaveChangesAsync();
                trans.Commit();
                req.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                if (customGenCodeId > 0)
                {
                    await _customGenCodeHelperService.ConfirmCode(customGenCodeId);
                }
                await _activityLogService.CreateLog(objectType, inventoryRequirement.InventoryRequirementId, $"Thêm mới dữ liệu yêu cầu xuất/nhập kho {inventoryRequirement.InventoryRequirementCode}", req.JsonSerialize());
                return req;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateInventoryRequirement");
                throw;
            }
        }

        public async Task<InventoryRequirementModel> UpdateInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, InventoryRequirementModel req)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;

                var inventoryRequirement = _stockDBContext.InventoryRequirement
                    .Include(r => r.InventoryRequirementDetail)
                    .Include(r => r.InventoryRequirementFile)
                    .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

                var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
                if (inventoryRequirement == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");

                if (inventoryRequirement.InventoryRequirementCode != req.InventoryRequirementCode)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được thay đổi mã phiếu yêu cầu");

                if (inventoryRequirement.ProductionHandoverId.HasValue)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được thay đổi phiếu yêu cầu từ sản xuất");

                _mapper.Map(req, inventoryRequirement);

                // Cập nhật file
                var createFiles = req.InventoryRequirementFile.Where(f => !inventoryRequirement.InventoryRequirementFile.Any(of => of.FileId == f.FileId)).ToList();
                var deleteFiles = inventoryRequirement.InventoryRequirementFile.Where(f => !req.InventoryRequirementFile.Any(of => of.FileId == f.FileId)).ToList();
                // Tạo mới
                foreach (var item in createFiles)
                {
                    var inventoryRequirementFile = _mapper.Map<InventoryRequirementFile>(item);
                    inventoryRequirementFile.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                    _stockDBContext.InventoryRequirementFile.Add(inventoryRequirementFile);
                }
                // Xóa
                foreach (var item in deleteFiles)
                {
                    item.IsDeleted = true;
                }

                // Cập nhật detail
                var newDetails = new List<InventoryRequirementDetailModel>(req.InventoryRequirementDetail);
                var oldDetails = new List<InventoryRequirementDetail>(inventoryRequirement.InventoryRequirementDetail);

                foreach (var item in newDetails)
                {
                    var entity = oldDetails.FirstOrDefault(d => d.InventoryRequirementDetailId == item.InventoryRequirementDetailId);
                    if (entity == null)
                    {
                        // Tạo mới
                        var inventoryRequirementDetail = _mapper.Map<InventoryRequirementDetail>(item);
                        inventoryRequirementDetail.InventoryRequirementId = inventoryRequirement.InventoryRequirementId;
                        _stockDBContext.InventoryRequirementDetail.Add(inventoryRequirementDetail);
                    }
                    else // Cập nhật
                    {
                        _mapper.Map(item, entity);
                        oldDetails.Remove(entity);
                    }
                }
                // Xóa
                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                }

                await _stockDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(objectType, inventoryRequirement.InventoryRequirementId, $"Cập nhật dữ liệu yêu cầu xuất/nhập kho {inventoryRequirement.InventoryRequirementCode}", req.JsonSerialize());
                return req;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateInventoryRequirement");
                throw;
            }
        }

        public async Task<bool> DeleteInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId)
        {
            using var trans = await _stockDBContext.Database.BeginTransactionAsync();
            try
            {
                var objectType = inventoryType == EnumInventoryType.Input ? EnumObjectType.InventoryInputRequirement : EnumObjectType.InventoryOutputRequirement;

                var inventoryRequirement = _stockDBContext.InventoryRequirement
                    .Include(r => r.InventoryRequirementDetail)
                    .Include(r => r.InventoryRequirementFile)
                    .FirstOrDefault(r => r.InventoryTypeId == (int)inventoryType && r.InventoryRequirementId == inventoryRequirementId);

                var type = inventoryType == EnumInventoryType.Input ? "nhập kho" : "xuất kho";
                if (inventoryRequirement == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Yêu cầu {type} không tồn tại");

                if (inventoryRequirement.ProductionHandoverId.HasValue)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không được xóa phiếu yêu cầu từ sản xuất");

                inventoryRequirement.IsDeleted = true;

                // Xóa file
                foreach (var item in inventoryRequirement.InventoryRequirementFile)
                {
                    item.IsDeleted = true;
                }

                // Xóa detail
                foreach (var item in inventoryRequirement.InventoryRequirementDetail)
                {
                    item.IsDeleted = true;
                }

                await _stockDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(objectType, inventoryRequirement.InventoryRequirementId, $"Xóa dữ liệu yêu cầu xuất/nhập kho {inventoryRequirement.InventoryRequirementCode}", inventoryRequirement.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteInventoryRequirement");
                throw;
            }
        }
    }
}
