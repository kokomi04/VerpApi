using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Employee;

namespace VErp.Services.Organization.Service.Employee
{
    public interface IEmployeeService
    {
        Task<ServiceResult<EmployeeModel>> GetInfo(int userId);
        Task<ServiceResult<int>> CreateEmployee(int userId, EmployeeModel req, int updatedUserId);
        Task<Enum> UpdateEmployee(int userId, EmployeeModel req, int updatedUserId);
        Task<Enum> DeleteEmployee(int userId);
    }
}
