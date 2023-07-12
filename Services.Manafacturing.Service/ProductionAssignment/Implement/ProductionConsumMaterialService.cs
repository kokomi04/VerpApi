using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment.Implement
{
    public class ProductionConsumMaterialService : IProductionConsumMaterialService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionConsumMaterialService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionConsumMaterialService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionConsumMaterial);
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<long> CreateConsumMaterial(int departmentId, long productionOrderId, long productionStepId, ProductionConsumMaterialModel model)
        {
            var materials = model?.Details?.Where(d => d.Key > 0 && d.Value?.Values?.Count() > 0)?.ToList();
            if (materials == null || materials.Count() == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập ít nhất một loại vật tư tiêu hao");
            }

            var assignmentInfo = _manufacturingDBContext.ProductionAssignment.FirstOrDefault(a => a.DepartmentId == departmentId && a.ProductionStepId == productionStepId && a.ProductionOrderId == productionOrderId);
            if (assignmentInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy phân công của công đoạn cho bộ phận này!");
            }

            var consumMaterial = _mapper.Map<ProductionConsumMaterial>(model);
            consumMaterial.DepartmentId = departmentId;
            consumMaterial.ProductionOrderId = productionOrderId;
            consumMaterial.ProductionStepId = productionStepId;

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                await _manufacturingDBContext.ProductionConsumMaterial.AddAsync(consumMaterial);
                await _manufacturingDBContext.SaveChangesAsync();

                var details = new List<ProductionConsumMaterialDetail>();
                foreach (var group in model.Details)
                {
                    foreach (var item in group.Value)
                    {
                        var detail = _mapper.Map<ProductionConsumMaterialDetail>(item.Value);
                        detail.ObjectId = item.Key;
                        detail.ObjectTypeId = group.Key;
                        detail.ProductionConsumMaterialId = consumMaterial.ProductionConsumMaterialId;
                        details.Add(detail);
                    }
                }

                await _manufacturingDBContext.ProductionConsumMaterialDetail.AddRangeAsync(details);
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Create)
                   .MessageResourceFormatDatas($"Khai báo tiêu hao vật tư từ ngày {consumMaterial?.FromDate?.ToString("dd/MM/yyyy")} đến ngày {consumMaterial?.ToDate?.ToString("dd/MM/yyyy")}")
                   .ObjectId(consumMaterial.ProductionConsumMaterialId)
                   .ObjectType(EnumObjectType.ProductionConsumMaterial)
                   .JsonData(new
                   {
                       departmentId,
                       productionOrderId,
                       productionStepId,
                       model
                   })
                   .CreateLog();
                return consumMaterial.ProductionConsumMaterialId;
            }
        }

        public async Task<IDictionary<long, List<ProductionConsumMaterialModel>>> GetConsumMaterials(int departmentId, long productionOrderId, long[] productionStepIds)
        {
            var consumMaterials = await _manufacturingDBContext.ProductionConsumMaterial
                .Include(c => c.ProductionConsumMaterialDetail)
                .Where(a => a.DepartmentId == departmentId && productionStepIds.Contains(a.ProductionStepId) && a.ProductionOrderId == productionOrderId)
                .ToListAsync();

            return consumMaterials
                .GroupBy(c => c.ProductionStepId)
                .ToDictionary(g => g.Key, g => g.Select(c =>
                    {
                        var consumMaterial = _mapper.Map<ProductionConsumMaterialModel>(c);
                        var details = new Dictionary<int, Dictionary<long, ProductionConsumMaterialDetailModel>>();
                        foreach (var detail in c.ProductionConsumMaterialDetail)
                        {
                            if (!details.ContainsKey(detail.ObjectTypeId))
                            {
                                details.Add(detail.ObjectTypeId, new Dictionary<long, ProductionConsumMaterialDetailModel>());
                            }
                            details[detail.ObjectTypeId].Add(detail.ObjectId, _mapper.Map<ProductionConsumMaterialDetailModel>(detail));
                        }
                        consumMaterial.Details = details;
                        return consumMaterial;
                    }).ToList()
                );
        }

        public async Task<bool> UpdateConsumMaterial(int departmentId, long productionOrderId, long productionStepId, long productionConsumMaterialId, ProductionConsumMaterialModel model)
        {
            var materials = model?.Details?.Where(d => d.Key > 0 && d.Value?.Values?.Count() > 0)?.ToList();
            if (materials == null || materials.Count() == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng nhập ít nhất một loại vật tư tiêu hao");
            }

            var consumMaterial = await _manufacturingDBContext.ProductionConsumMaterial.FirstOrDefaultAsync(s => s.ProductionConsumMaterialId == productionConsumMaterialId);
            if (consumMaterial == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy khai báo vật tư tiêu hao trong hệ thống");
            }

            if (consumMaterial.DepartmentId != departmentId || consumMaterial.ProductionOrderId != productionOrderId || consumMaterial.ProductionStepId != productionStepId)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                _mapper.Map(model, consumMaterial);

                var oldDetails = await _manufacturingDBContext.ProductionConsumMaterialDetail
                    .Where(d => d.ProductionConsumMaterialId == productionConsumMaterialId)
                    .ToListAsync();

                foreach (var detail in oldDetails)
                {
                    detail.IsDeleted = true;
                }

                var details = new List<ProductionConsumMaterialDetail>();
                foreach (var group in model.Details)
                {
                    foreach (var item in group.Value)
                    {
                        var detail = _mapper.Map<ProductionConsumMaterialDetail>(item.Value);
                        detail.ObjectId = item.Key;
                        detail.ObjectTypeId = group.Key;
                        detail.ProductionConsumMaterialId = consumMaterial.ProductionConsumMaterialId;
                        details.Add(detail);
                    }
                }

                await _manufacturingDBContext.ProductionConsumMaterialDetail.AddRangeAsync(details);
                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Update)
                   .MessageResourceFormatDatas($"Cập nhật khai báo vật tư tiêu hao từ ngày {consumMaterial?.FromDate?.ToString("dd/MM/yyyy")} đến ngày {consumMaterial?.ToDate?.ToString("dd/MM/yyyy")}")
                   .ObjectId(productionConsumMaterialId)
                   .ObjectType(EnumObjectType.ProductionConsumMaterial)
                   .JsonData(new
                   {
                       productionConsumMaterialId,
                       model
                   })
                   .CreateLog();
                return true;
            }
        }


        public async Task<bool> DeleteConsumMaterial(int departmentId, long productionOrderId, long productionStepId, long productionConsumMaterialId)
        {

            var consumMaterial = await _manufacturingDBContext.ProductionConsumMaterial
                .FirstOrDefaultAsync(s => s.ProductionConsumMaterialId == productionConsumMaterialId);
            if (consumMaterial == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy khai báo tiêu hao trong hệ thống");
            }


            if (consumMaterial.DepartmentId != departmentId || consumMaterial.ProductionOrderId != productionOrderId || consumMaterial.ProductionStepId != productionStepId)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                consumMaterial.IsDeleted = true;

                //var details = await _manufacturingDBContext.ProductionScheduleTurnShiftUser.Where(u => u.ProductionScheduleTurnShiftId == productionConsumMaterialId).ToListAsync();
                //foreach (var u in details)
                //{
                //    u.IsDeleted = true;
                //}

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Delete)
                   .MessageResourceFormatDatas($"Xóa khai báo vật tư tiêu hao từ ngày {consumMaterial?.FromDate?.ToString("dd/MM/yyyy")} đến ngày {consumMaterial?.ToDate?.ToString("dd/MM/yyyy")}")
                   .ObjectId(productionConsumMaterialId)
                   .ObjectType(EnumObjectType.ProductionConsumMaterial)
                   .JsonData(new
                   {
                       productionConsumMaterialId,
                   })
                   .CreateLog();
                return true;
            }
        }

        public async Task<bool> DeleteMaterial(int departmentId, long productionOrderId, long productionStepId, int objectTypeId, long objectId)
        {
            var consumMaterialDetails = (from sd in _manufacturingDBContext.ProductionConsumMaterialDetail
                                         join s in _manufacturingDBContext.ProductionConsumMaterial
                                         on sd.ProductionConsumMaterialId equals s.ProductionConsumMaterialId
                                         where s.DepartmentId == departmentId
                                         && s.ProductionOrderId == productionOrderId
                                         && s.ProductionStepId == productionStepId
                                         && sd.ObjectTypeId == objectTypeId
                                         && sd.ObjectId == objectId
                                         select sd).ToList();

            foreach (var consumMaterialDetail in consumMaterialDetails)
            {
                consumMaterialDetail.IsDeleted = true;
                await _objActivityLogFacade.LogBuilder(() => ActionButtonActivityLogMessage.Delete)
                   .MessageResourceFormatDatas($"Xóa khai báo vật tư tiêu hao")
                   .ObjectId(objectId)
                   .ObjectType(EnumObjectType.ProductionConsumMaterial)
                   .JsonData(new
                   {
                       departmentId,
                       productionOrderId,
                       productionStepId,
                       objectTypeId,
                       objectId
                   })
                   .CreateLog();
            }

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
    }
}
