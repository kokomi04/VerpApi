﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.DepartmentCalendar;

namespace VErp.Services.Organization.Service.DepartmentCalendar
{
    public interface IDepartmentCalendarService
    {
        Task<PageData<DepartmentCalendarModel>> GetDepartmentCalendars(int departmentId, int page, int size);
        Task<DepartmentCalendarModel> CreateDepartmentCalendar(int departmentId, DepartmentCalendarModel data);
        Task<DepartmentCalendarModel> UpdateDepartmentCalendar(int departmentId, long oldDate, DepartmentCalendarModel data);
        Task<bool> DeleteDepartmentCalendar(int departmentId, long startDate);


        Task<IList<DepartmentCalendarListModel>> GetListDepartmentCalendar(int[] departmentIds, long startDate, long endDate);



        Task<PageData<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo(int departmentId, int page, int size);
        Task<DepartmentOverHourInfoModel> CreateDepartmentOverHourInfo(int departmentId, DepartmentOverHourInfoModel data);
        Task<DepartmentOverHourInfoModel> UpdateDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId, DepartmentOverHourInfoModel data);
        Task<IList<DepartmentOverHourInfoModel>> UpdateDepartmentOverHourInfoMultiple(IList<DepartmentOverHourInfoModel> data);
        Task<bool> DeleteDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId);
        Task<IList<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo(int[] departmentIds);


        Task<PageData<DepartmentIncreaseInfoModel>> GetDepartmentIncreaseInfo(int departmentId, int page, int size);
        Task<DepartmentIncreaseInfoModel> CreateDepartmentIncreaseInfo(int departmentId, DepartmentIncreaseInfoModel data);
        Task<DepartmentIncreaseInfoModel> UpdateDepartmentIncreaseInfo(int departmentId, long departmentIncreaseInfoId, DepartmentIncreaseInfoModel data);
        Task<IList<DepartmentIncreaseInfoModel>> UpdateDepartmentIncreaseInfoMultiple(IList<DepartmentIncreaseInfoModel> data);
        Task<bool> DeleteDepartmentIncreaseInfo(int departmentId, long departmentIncreaseInfoId);
        Task<IList<DepartmentIncreaseInfoModel>> GetDepartmentIncreaseInfo(int[] departmentIds);

    }
}
