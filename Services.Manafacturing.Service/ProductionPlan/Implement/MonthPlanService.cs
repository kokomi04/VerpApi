using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.MonthPlan;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{
    public class MonthPlanService : IMonthPlanService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public MonthPlanService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<MonthPlanService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionPlan);
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<MonthPlanModel> CreateMonthPlan(MonthPlanModel data)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (data.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu.");
                if (data.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc.");

                if (data.StartDate > data.EndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được phép sau ngày kết thúc.");
                var monthPlan = _mapper.Map<MonthPlan>(data);

                if (_manufacturingDBContext.MonthPlan.Any(mp => mp.StartDate <= monthPlan.EndDate && mp.EndDate >= monthPlan.StartDate))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch đã bị trùng lặp. Vui lòng kiểm tra lại.");
                }

                _manufacturingDBContext.MonthPlan.Add(monthPlan);
                _manufacturingDBContext.SaveChanges();

                // Tạo kế hoạch tuần
                if (data.WeekPlans.Any(wp => wp.StartDate <= 0 || wp.EndDate <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập thời gian kế hoạch tuần.");

                if (data.WeekPlans.Any(wp => wp.StartDate > wp.EndDate)) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch tuần đang có ngày bắt đầu sau ngày kết thúc.");

                if (data.WeekPlans.Any(wp => data.WeekPlans.Any(owp => owp != wp && wp.StartDate <= owp.EndDate && wp.EndDate >= owp.StartDate)))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch tuần đã bị trùng lặp. Vui lòng kiểm tra lại.");
                }

                var monthDays = monthPlan.EndDate.Subtract(monthPlan.StartDate).TotalDays + 1;
                var weekDays = data.WeekPlans.Sum(wp => wp.EndDate.UnixToDateTime().Value.Subtract(wp.StartDate.UnixToDateTime().Value).TotalDays + 1);
                if (weekDays != monthDays)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian kế hoạch tháng chưa phân đủ vào kế hoạch tuần.");

                foreach (var item in data.WeekPlans)
                {
                    var weekPlan = _mapper.Map<WeekPlan>(item);
                    weekPlan.MonthPlanId = monthPlan.MonthPlanId;

                    if (weekPlan.StartDate < monthPlan.StartDate || weekPlan.EndDate > monthPlan.EndDate)
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch tuần không được nằm ngoài kế hoạch tháng. Vui lòng kiểm tra lại.");

                    if (_manufacturingDBContext.WeekPlan.Any(wp => wp.StartDate <= weekPlan.EndDate && wp.EndDate >= weekPlan.StartDate))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch tuần đã bị trùng lặp. Vui lòng kiểm tra lại.");
                    }
                    _manufacturingDBContext.WeekPlan.Add(weekPlan);
                }

                _manufacturingDBContext.SaveChanges();
                data.MonthPlanId = monthPlan.MonthPlanId;
                trans.Commit();
                await _objActivityLogFacade.LogBuilder(() => MonthPlanActivityLogMessage.Create)
                   .MessageResourceFormatDatas(monthPlan.MonthPlanName)
                   .ObjectId(monthPlan.MonthPlanId)
                   .ObjectType(EnumObjectType.ProductionPlan)
                   .JsonData(data)
                   .CreateLog();
                return data;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateMonthPlan");
                throw;
            }
        }

        public async Task<MonthPlanModel> UpdateMonthPlan(int monthPlanId, MonthPlanModel data)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (data.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu.");
                if (data.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc.");

                if (data.StartDate > data.EndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được phép sau ngày kết thúc.");

                var monthPlan = _manufacturingDBContext.MonthPlan.FirstOrDefault(mp => mp.MonthPlanId == monthPlanId);
                if (monthPlan == null) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch tháng không tồn tại.");

                _mapper.Map(data, monthPlan);
                monthPlan.MonthPlanId = monthPlanId;
                if (_manufacturingDBContext.MonthPlan.Any(mp => mp.MonthPlanId != monthPlanId && mp.StartDate <= monthPlan.EndDate && mp.EndDate >= monthPlan.StartDate))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch đã bị trùng lặp. Vui lòng kiểm tra lại.");
                }
                _manufacturingDBContext.SaveChanges();

                // Update kế hoạch tuần
                if (data.WeekPlans.Any(wp => wp.StartDate <= 0 || wp.EndDate <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập thời gian kế hoạch tuần.");

                if (data.WeekPlans.Any(wp => wp.StartDate > wp.EndDate)) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch tuần đang có ngày bắt đầu sau ngày kết thúc.");

                if (data.WeekPlans.Any(wp => data.WeekPlans.Any(owp => owp != wp && wp.StartDate <= owp.EndDate && wp.EndDate >= owp.StartDate)))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch tuần đã bị trùng lặp. Vui lòng kiểm tra lại.");
                }

                var monthDays = monthPlan.EndDate.Subtract(monthPlan.StartDate).TotalDays + 1;
                var weekDays = data.WeekPlans.Sum(wp => wp.EndDate.UnixToDateTime().Value.Subtract(wp.StartDate.UnixToDateTime().Value).TotalDays + 1);
                if (weekDays != monthDays)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian kế hoạch tháng chưa phân đủ vào kế hoạch tuần.");

                var currentWeekPlans = _manufacturingDBContext.WeekPlan.Where(wp => wp.MonthPlanId == monthPlanId).ToList();
                foreach (var item in data.WeekPlans)
                {
                    var weekPlan = currentWeekPlans.FirstOrDefault(wp => wp.WeekPlanId == item.WeekPlanId);
                    // Tạo mới
                    if (weekPlan == null)
                    {
                        weekPlan = _mapper.Map<WeekPlan>(item);
                        weekPlan.MonthPlanId = monthPlan.MonthPlanId;
                        _manufacturingDBContext.WeekPlan.Add(weekPlan);
                    }
                    else // Cập nhâtk
                    {
                        _mapper.Map(item, weekPlan);
                        currentWeekPlans.Remove(weekPlan);
                    }
                    if (weekPlan.StartDate < monthPlan.StartDate || weekPlan.EndDate > monthPlan.EndDate)
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch tuần không được nằm ngoài kế hoạch tháng. Vui lòng kiểm tra lại.");

                    if (_manufacturingDBContext.WeekPlan.Any(wp => wp.MonthPlanId != monthPlanId && wp.StartDate <= weekPlan.EndDate && wp.EndDate >= weekPlan.StartDate))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian của kế hoạch tuần đã bị trùng lặp. Vui lòng kiểm tra lại.");
                    }
                }

                // Xóa
                var toDeleteWeekPlanIds = currentWeekPlans.Select(w => (int?)w.WeekPlanId).ToList();
                if (toDeleteWeekPlanIds.Count > 0)
                {
                    var usedProductionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(p => toDeleteWeekPlanIds.Contains(p.FromWeekPlanId) || toDeleteWeekPlanIds.Contains(p.ToWeekPlanId));
                    if (usedProductionOrder != null)
                    {
                        var weekInfo = currentWeekPlans.FirstOrDefault(w => w.WeekPlanId == usedProductionOrder.FromWeekPlanId || w.WeekPlanId == usedProductionOrder.ToWeekPlanId);
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể xóa Kế hoạch tuần {weekInfo?.WeekPlanName} đang được sử dụng bởi lệnh: " + usedProductionOrder.ProductionOrderCode);
                    }

                    foreach (var item in currentWeekPlans)
                    {
                        item.IsDeleted = true;
                    }
                }

                _manufacturingDBContext.SaveChanges();
                trans.Commit();
                await _objActivityLogFacade.LogBuilder(() => MonthPlanActivityLogMessage.Update)
                   .MessageResourceFormatDatas(monthPlan.MonthPlanName)
                   .ObjectId(monthPlan.MonthPlanId)
                   .ObjectType(EnumObjectType.ProductionPlan)
                   .JsonData(data)
                   .CreateLog();
                return data;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateMonthPlan");
                throw;
            }
        }

        public async Task<bool> DeleteMonthPlan(int monthPlanId)
        {
            try
            {
                var usedProductionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(p => p.MonthPlanId == monthPlanId);
                if (usedProductionOrder != null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa Kế hoạch tháng đang được sử dụng bởi lệnh: " + usedProductionOrder.ProductionOrderCode);
                }

                var monthPlan = _manufacturingDBContext.MonthPlan.Where(mp => mp.MonthPlanId == monthPlanId).FirstOrDefault();

                if (monthPlan == null) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch tháng không tồn tại");

                var weekPlans = _manufacturingDBContext.WeekPlan.Where(wp => wp.MonthPlanId == monthPlanId).ToList();

                foreach (var item in weekPlans)
                {
                    item.IsDeleted = true;
                }
                monthPlan.IsDeleted = true;
                _manufacturingDBContext.SaveChanges();
                await _objActivityLogFacade.LogBuilder(() => MonthPlanActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(monthPlan.MonthPlanName)
                   .ObjectId(monthPlan.MonthPlanId)
                   .ObjectType(EnumObjectType.ProductionPlan)
                   .JsonData(monthPlan)
                   .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteMonthPlan");
                throw;
            }
        }


        public async Task<MonthPlanModel> GetMonthPlan(int monthPlanId)
        {
            var monthPlan = await _manufacturingDBContext.MonthPlan.Where(mp => mp.MonthPlanId == monthPlanId).FirstOrDefaultAsync();
            if (monthPlan == null) return null;
            var model = _mapper.Map<MonthPlanModel>(monthPlan);
            model.WeekPlans = await _manufacturingDBContext.WeekPlan
                .Where(wp => wp.MonthPlanId == monthPlanId)
                .OrderBy(wp => wp.StartDate)
                .ProjectTo<WeekPlanModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return model;
        }

        public async Task<MonthPlanModel> GetMonthPlan(string monthPlanName)
        {
            var monthPlan = await _manufacturingDBContext.MonthPlan.Where(mp => mp.MonthPlanName == monthPlanName).FirstOrDefaultAsync();

            if (monthPlan == null) return null;
            var model = _mapper.Map<MonthPlanModel>(monthPlan);
            model.WeekPlans = await _manufacturingDBContext.WeekPlan
                .Where(wp => wp.MonthPlanId == monthPlan.MonthPlanId)
                .OrderBy(wp => wp.StartDate)
                .ProjectTo<WeekPlanModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return model;
        }


        public async Task<MonthPlanModel> GetMonthPlan(long startDate, long endDate)
        {
            var monthPlan = await _manufacturingDBContext.MonthPlan
                .Where(mp => mp.StartDate <= endDate.UnixToDateTime() && mp.EndDate >= startDate.UnixToDateTime())
                .OrderBy(mp => mp.StartDate)
                .FirstOrDefaultAsync();
            if (monthPlan == null) return null;
            var model = _mapper.Map<MonthPlanModel>(monthPlan);
            model.WeekPlans = await _manufacturingDBContext.WeekPlan
                .Where(wp => wp.MonthPlanId == monthPlan.MonthPlanId)
                .OrderByDescending(wp => wp.StartDate)
                .ProjectTo<WeekPlanModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return model;
        }



        public async Task<PageData<MonthPlanModel>> GetMonthPlans(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var monthPlans = _manufacturingDBContext.MonthPlan.AsQueryable();


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                monthPlans = monthPlans.Where(mp => mp.MonthPlanName.Contains(keyword) || mp.MonthNote.Contains(keyword));
            }
            if (filters != null)
            {
                monthPlans = monthPlans.InternalFilter(filters);
            }
            if (string.IsNullOrEmpty(orderByFieldName))
            {
                orderByFieldName = "StartDate";
                asc = false;
            }
            monthPlans = monthPlans.InternalOrderBy(orderByFieldName, asc);
            var total = await monthPlans.CountAsync();

            monthPlans = size > 0 ? monthPlans.Skip((page - 1) * size).Take(size) : monthPlans;
            var weekPlans = await (from w in _manufacturingDBContext.WeekPlan
                                   join m in monthPlans on w.MonthPlanId equals m.MonthPlanId
                                   select w)
                            .ProjectTo<WeekPlanModel>(_mapper.ConfigurationProvider)
                            .ToListAsync();

            var lstMonthPlans = monthPlans.ProjectTo<MonthPlanModel>(_mapper.ConfigurationProvider).ToList();

            foreach (var m in lstMonthPlans)
            {
                m.WeekPlans = weekPlans.Where(w => w.MonthPlanId == m.MonthPlanId).ToList();
            }
            return (lstMonthPlans, total);
        }

    }
}
