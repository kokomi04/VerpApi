﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Department;

namespace VErp.Services.Organization.Service.Department
{
    public interface IDepartmentService
    {
        Task<ServiceResult<int>> AddDepartment(int updatedUserId, DepartmentModel data);
        Task<PageData<DepartmentModel>> GetList(string keyword, bool? isActived, int page, int size, Dictionary<string, List<string>> filters = null);
        Task<ServiceResult<DepartmentModel>> GetDepartmentInfo(int departmentId);
        Task<Enum> UpdateDepartment(int updatedUserId, int departmentId, DepartmentModel data);
        Task<Enum> DeleteDepartment(int updatedUserId, int departmentId);
    }
}