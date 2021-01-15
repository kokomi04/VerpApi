using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Department;

namespace VErp.Services.Organization.Service.Department
{
    public interface IDepartmentService
    {
        Task<int> AddDepartment(int updatedUserId, DepartmentModel data);
        Task<PageData<DepartmentModel>> GetList(string keyword, bool? isActived, int page, int size, Clause filters = null);
        Task<IList<DepartmentModel>> GetListByIds(IList<int> departmentIds);
        Task<DepartmentModel> GetDepartmentInfo(int departmentId);
        Task<bool> UpdateDepartment(int updatedUserId, int departmentId, DepartmentModel data);
        Task<bool> DeleteDepartment(int updatedUserId, int departmentId);
    }
}
