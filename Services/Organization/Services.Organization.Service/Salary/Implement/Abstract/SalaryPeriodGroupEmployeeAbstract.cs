using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Abstract;

namespace VErp.Services.Organization.Service.Salary.Implement.Abstract
{
    public abstract class SalaryPeriodGroupEmployeeAbstract : BillDateValidateionServiceAbstract
    {
        protected readonly OrganizationDBContext _organizationDBContext;
        protected readonly ICurrentContextService _currentContextService;
        protected readonly ILogger _logger;
        protected SalaryPeriodGroupEmployeeAbstract(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, ILogger logger)
            : base(organizationDBContext)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _logger = logger;
        }

        protected async Task<bool> DeleteSalaryEmployeeByPeriodGroup(int salaryPeriodId, int? salaryGroupId)
        {

            var subsidiaryInfo = await _organizationDBContext.Subsidiary.FirstOrDefaultAsync(s => s.SubsidiaryId == _currentContextService.SubsidiaryId);
            if (subsidiaryInfo != null)
            {
                var sql = $"UPDATE {GetEmployeeSalaryTableName(subsidiaryInfo.SubsidiaryCode)} SET IsDeleted = 1, DeletedDatetimeUtc = GETUTCDATE(), UpdatedByUserId = @UserId WHERE SalaryPeriodId = @SalaryPeriodId AND (SalaryGroupId = @SalaryGroupId OR @SalaryGroupId IS NULL)";
                var sqlParams = new[]
                {
                    new SqlParameter("@UserId", _currentContextService.UserId),
                    new SqlParameter("@SalaryPeriodId", salaryPeriodId),
                    new SqlParameter("@SalaryGroupId", salaryGroupId==null?DBNull.Value:salaryGroupId),
                };
                await _organizationDBContext.Database.ExecuteSqlRawAsync(sql, sqlParams);
            }
            return true;
        }

        public string GetEmployeeSalaryTableName(string subsidiaryCode)
        {
            return OrganizationConstants.GetEmployeeSalaryTableName(subsidiaryCode);
        }
    }
}
