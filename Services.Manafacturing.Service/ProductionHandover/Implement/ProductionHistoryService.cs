using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class ProductionHistoryService : IProductionHistoryService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private const int STOCK_DEPARTMENT_ID = -1;
        public ProductionHistoryService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionHistoryService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<ProductionHistoryModel> CreateProductionHistory(long productionOrderId, ProductionHistoryInputModel data)
        {
            try
            {
                if (data.DepartmentId == STOCK_DEPARTMENT_ID && data.DepartmentId == STOCK_DEPARTMENT_ID)
                {
                    if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == data.ProductionStepId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");
                    if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == data.ProductionStepId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không có gia công công đoạn");
                }
                else
                {
                    if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.ProductionStepId && a.DepartmentId == data.DepartmentId && a.ProductionOrderId == productionOrderId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                    if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.ProductionStepId && a.DepartmentId == data.DepartmentId && a.ProductionOrderId == productionOrderId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không tồn tại phân công công việc cho tổ nhận");
                }

                var productionHistory = _mapper.Map<ProductionHistory>(data);
                productionHistory.ProductionOrderId = productionOrderId;
                _manufacturingDBContext.ProductionHistory.Add(productionHistory);
                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionHistory, productionHistory.ProductionHistoryId, $"Tạo lịch sử sản xuất", data.JsonSerialize());
                return _mapper.Map<ProductionHistoryModel>(productionHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductionHistory");
                throw;
            }
        }

        public async Task<bool> DeleteProductionHistory(long productionHistoryId)
        {
            try
            {
                var productionHistory = _manufacturingDBContext.ProductionHistory
                    .Where(h => h.ProductionHistoryId == productionHistoryId)
                    .FirstOrDefault();

                if (productionHistory == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại lịch sử sản xuất");
                productionHistory.IsDeleted = true;
                _manufacturingDBContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.ProductionHistory, productionHistoryId, $"Xoá lịch sử sản xuất", productionHistory.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProductionHistory");
                throw;
            }
        }

        public async Task<IList<ProductionHistoryModel>> CreateMultipleProductionHistory(long productionOrderId, IList<ProductionHistoryInputModel> data)
        {
            var insertData = new List<ProductionHistory>();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in data)
                {
                    if (item.DepartmentId == STOCK_DEPARTMENT_ID && item.DepartmentId == STOCK_DEPARTMENT_ID)
                    {
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == item.ProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == item.ProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không có gia công công đoạn");
                    }
                    else
                    {
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == item.ProductionStepId && a.DepartmentId == item.DepartmentId && a.ProductionOrderId == productionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == item.ProductionStepId && a.DepartmentId == item.DepartmentId && a.ProductionOrderId == productionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không tồn tại phân công công việc cho tổ nhận");
                    }

                    var productionHistory = _mapper.Map<ProductionHistory>(item);
                    productionHistory.ProductionOrderId = productionOrderId;
                    _manufacturingDBContext.ProductionHistory.Add(productionHistory);

                    insertData.Add(productionHistory);
                }

                _manufacturingDBContext.SaveChanges();

                var result = insertData.AsQueryable().ProjectTo<ProductionHistoryModel>(_mapper.ConfigurationProvider).ToList();

                foreach (var item in insertData)
                {
                    await _activityLogService.CreateLog(EnumObjectType.ProductionHistory, item.ProductionHistoryId, $"Tạo lịch sử sản xuất", data.JsonSerialize());
                }

                trans.Commit();

                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateMultipleProductionHistory");
                throw;
            }
        }


        public async Task<IList<ProductionHistoryModel>> GetProductionHistories(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionHistory
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionHistoryModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

        }




        public async Task<IDictionary<long, ActualWorkloadModel>> GetActualWorkloadByDate(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var productionOrders = _manufacturingDBContext.ProductionOrder.Where(po => po.StartDate <= toDateTime && po.PlanEndDate >= fromDateTime).ToList();
            var productionOrderIds = productionOrders.Select(po => po.ProductionOrderId).ToList();

            productionOrderIds = productionOrderIds.Distinct().ToList();

            var productionHistories = await (from ph in _manufacturingDBContext.ProductionHistory
                                       join g in _manufacturingDBContext.ProductionStep on ph.ProductionStepId equals g.ProductionStepId
                                       join ps in _manufacturingDBContext.ProductionStep on g.ParentId equals ps.ProductionStepId
                                       where productionOrderIds.Contains(ph.ProductionOrderId) && ps.StepId.HasValue
                                       select new
                                       {
                                           StepId = ps.StepId.Value,
                                           ph.ObjectId,
                                           ph.ObjectTypeId,
                                           ph.ProductionQuantity,
                                           Date = ph.Date.Value
                                       }).ToListAsync();

            var result = productionHistories
                .GroupBy(ph => ph.Date.GetUnix())
                .ToDictionary(g => g.Key, g =>
                {
                    var actualWorkload = new ActualWorkloadModel();
                    actualWorkload.ActualWorkloadOutput = g
                        .GroupBy(ph => ph.StepId)
                        .ToDictionary(sg => sg.Key, sg => sg.Select(ph => new ActualWorkloadOutputModel
                        {
                            ObjectId = ph.ObjectId,
                            ObjectTypeId = ph.ObjectTypeId,
                            Quantity = ph.ProductionQuantity
                        }).ToList());


                    return actualWorkload;
                });


            return result;
        }

        public async Task<IDictionary<long, ActualWorkloadModel>> GetCompletionActualWorkload(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var productionOrders = _manufacturingDBContext.ProductionOrder.Where(po => po.StartDate <= toDateTime && po.PlanEndDate >= fromDateTime).ToList();
            var productionOrderIds = productionOrders.Select(po => po.ProductionOrderId).ToList();

            productionOrderIds = productionOrderIds.Distinct().ToList();

            var productionHistories = await (from ph in _manufacturingDBContext.ProductionHistory
                                             join g in _manufacturingDBContext.ProductionStep on ph.ProductionStepId equals g.ProductionStepId
                                             join ps in _manufacturingDBContext.ProductionStep on g.ParentId equals ps.ProductionStepId
                                             where productionOrderIds.Contains(ph.ProductionOrderId) && ps.StepId.HasValue
                                             select new
                                             {
                                                 ph.ProductionOrderId,
                                                 StepId = ps.StepId.Value,
                                                 ph.ObjectId,
                                                 ph.ObjectTypeId,
                                                 ph.ProductionQuantity
                                             }).ToListAsync();

            var result = productionHistories
                .GroupBy(ph => ph.ProductionOrderId)
                .ToDictionary(g => g.Key, g =>
                {
                    var actualWorkload = new ActualWorkloadModel();
                    actualWorkload.ActualWorkloadOutput = g
                        .GroupBy(ph => ph.StepId)
                        .ToDictionary(sg => sg.Key, sg => sg.Select(ph => new ActualWorkloadOutputModel
                        {
                            ObjectId = ph.ObjectId,
                            ObjectTypeId = ph.ObjectTypeId,
                            Quantity = ph.ProductionQuantity
                        }).ToList());


                    return actualWorkload;
                });


            return result;
        }


    }
}
