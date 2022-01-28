using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan
{
    public interface IMonthPlanService
    {
        Task<PageData<MonthPlanModel>> GetMonthPlans(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<MonthPlanModel> GetMonthPlan(int monthPlanId);
        Task<MonthPlanModel> GetMonthPlan(string monthPlanName);
        Task<MonthPlanModel> GetMonthPlan(long startDate, long endDate);
        Task<MonthPlanModel> UpdateMonthPlan(int monthPlanId, MonthPlanModel data);
        Task<MonthPlanModel> CreateMonthPlan(MonthPlanModel data);
        Task<bool> DeleteMonthPlan(int monthPlanId);
    }
}
