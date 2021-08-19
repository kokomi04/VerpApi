using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.DepartmentCalendar;

namespace VErp.Services.Organization.Service.DepartmentCalendar
{
    public interface IDepartmentCalendarService
    {
        Task<DepartmentWeekCalendarModel> GetCurrentDepartmentCalendar(int departmentId);
        Task<DepartmentWeekCalendarModel> UpdateDepartmentWeekCalendar(int departmentId, DepartmentWeekCalendarModel data);

        Task<IList<DepartmentDayOffCalendarModel>> GetDepartmentDayOffCalendar(int departmentId, long startDate, long endDate);

        Task<DepartmentDayOffCalendarModel> UpdateDepartmentDayOff(int departmentId, DepartmentDayOffCalendarModel data);

        Task<bool> DeleteDepartmentDayOff(int departmentId, long day);

        Task<IList<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo(int departmentId);
        Task<DepartmentOverHourInfoModel> CreateDepartmentOverHourInfo(int departmentId, DepartmentOverHourInfoModel data);
        Task<DepartmentOverHourInfoModel> UpdateDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId, DepartmentOverHourInfoModel data);
        Task<bool> DeleteDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId);


    }
}
