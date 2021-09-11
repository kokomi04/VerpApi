using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{
    public class MonthPlanService : IMonthPlanService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public MonthPlanService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<MonthPlanService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
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
                if(weekDays != monthDays) 
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
                await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, monthPlan.MonthPlanId, $"Thêm mới kế hoạch tháng {monthPlan.MonthPlanName}", data.JsonSerialize());
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
                foreach (var item in currentWeekPlans)
                {
                    item.IsDeleted = true;
                }
                _manufacturingDBContext.SaveChanges();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, monthPlan.MonthPlanId, $"Cập nhật kế hoạch tháng {monthPlan.MonthPlanName}", data.JsonSerialize());
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
                var monthPlan = _manufacturingDBContext.MonthPlan.Where(mp => mp.MonthPlanId == monthPlanId).FirstOrDefault();

                if (monthPlan == null) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch tháng không tồn tại");

                var weekPlans = _manufacturingDBContext.WeekPlan.Where(wp => wp.MonthPlanId == monthPlanId).ToList();

                foreach (var item in weekPlans)
                {
                    item.IsDeleted = true;
                }
                monthPlan.IsDeleted = true;
                _manufacturingDBContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, monthPlanId, $"Xóa kế hoạch tháng {monthPlan.MonthPlanName}", monthPlan.JsonSerialize());
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
            if(monthPlan == null) return null;
            var model = _mapper.Map<MonthPlanModel>(monthPlan);
            model.WeekPlans = await _manufacturingDBContext.WeekPlan
                .Where(wp => wp.MonthPlanId == monthPlanId)
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
                .ProjectTo<WeekPlanModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return model;
        }


        public async Task<MonthPlanModel> GetMonthPlan(long startDate, long endDate)
        {
            var monthPlan = await _manufacturingDBContext.MonthPlan
                .Where(mp => mp.StartDate <= endDate.UnixToDateTime() && mp.EndDate >= startDate.UnixToDateTime())
                .FirstOrDefaultAsync();
            if (monthPlan == null) return null;
            var model = _mapper.Map<MonthPlanModel>(monthPlan);
            model.WeekPlans = await _manufacturingDBContext.WeekPlan
                .Where(wp => wp.MonthPlanId == monthPlan.MonthPlanId)
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
                monthPlans = monthPlans.InternalFilter(filters).InternalOrderBy(orderByFieldName, asc);
            }

            var total = await monthPlans.CountAsync();

            var lstMonthPlans = (size > 0 ? monthPlans.Skip((page - 1) * size).Take(size) : monthPlans).ProjectTo<MonthPlanModel>(_mapper.ConfigurationProvider).ToList();

            return (lstMonthPlans, total);
        }

    }
}
