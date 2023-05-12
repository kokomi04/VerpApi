using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionBillService
    {
        IQueryable<SalaryPeriodAdditionBill> GetListQuery(int salaryPeriodAdditionTypeId, int? year, int? month, string keyword);
        Task<PageData<SalaryPeriodAdditionBillList>> GetList(int salaryPeriodAdditionTypeId, int? year, int? month, string keyword, int page, int size);
        Task<SalaryPeriodAdditionBillInfo> GetInfo(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId);
        IQueryable<SalaryPeriodAdditionBill> QueryFullInfo();
        SalaryPeriodAdditionBillInfo MapInfo(SalaryPeriodAdditionBill fullInfo);
        Task<long> Create(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionBillModel model);
        Task<SalaryPeriodAdditionBill> CreateToDb(SalaryPeriodAdditionType typFullInfo, SalaryPeriodAdditionBillModel model, List<NonCamelCaseDictionary> emplyees);
        Task<bool> Update(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model);
        Task<bool> UpdateToDb(SalaryPeriodAdditionType typFullInfo, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model, List<NonCamelCaseDictionary> emplyees);
        Task<bool> Delete(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId);
    }
}
