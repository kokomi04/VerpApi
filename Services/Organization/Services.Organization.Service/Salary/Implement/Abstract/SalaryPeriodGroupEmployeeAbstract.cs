using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.Salary.Implement.Abstract
{
    public abstract class SalaryPeriodGroupEmployeeAbstract
    {
        protected OrganizationDBContext _organizationDBContext;
        protected ICurrentContextService _currentContextService;
        protected SalaryPeriodGroupEmployeeAbstract(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
        }

        protected async Task<bool> DeleteSalaryEmployeeByPeriodGroup(int salaryPeriodId, int salaryGroupId)
        {
            var salaryData = _organizationDBContext.SalaryEmployee.Where(e => e.SalaryPeriodId == salaryPeriodId && e.SalaryGroupId == salaryGroupId);
            await salaryData.UpdateByBatch(e =>
            new SalaryEmployee
            {
                IsDeleted = true,
                DeletedDatetimeUtc = DateTime.UtcNow,
                UpdatedByUserId = _currentContextService.UserId
            });
            await _organizationDBContext.SaveChangesAsync();
            return true;
        }
    }
}
