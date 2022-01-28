using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{

    public class DepartmentCalendarSimpleModel
    {
        public int DepartmentId { get; set; }
        public ICollection<DepartmentWorkingHourSimpleInfoModel> DepartmentWorkingHourInfo { get; set; }
        public ICollection<DepartmentOverHourSimpleInfoModel> DepartmentOverHourInfo { get; set; }
        public ICollection<DepartmentIncreaseSimpleInfoModel> DepartmentIncreaseInfo { get; set; }
        public DepartmentCalendarSimpleModel()
        {
            DepartmentWorkingHourInfo = new List<DepartmentWorkingHourSimpleInfoModel>();
            DepartmentOverHourInfo = new List<DepartmentOverHourSimpleInfoModel>();
            DepartmentIncreaseInfo = new List<DepartmentIncreaseSimpleInfoModel>();
        }
    }

    public class DepartmentWorkingHourSimpleInfoModel
    {
        public int DepartmentId { get; set; }
        public double WorkingHourPerDay { get; set; }
        public long StartDate { get; set; }
    }

    public class DepartmentOverHourSimpleInfoModel
    {
        public long DepartmentOverHourInfoId { get; set; }
        public int DepartmentId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public double OverHour { get; set; }
        public int NumberOfPerson { get; set; }
        public string Content { get; set; }
    }
    public class DepartmentIncreaseSimpleInfoModel
    {
        public long DepartmentIncreaseInfoId { get; set; }
        public int DepartmentId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int NumberOfPerson { get; set; }
        public string Content { get; set; }
    }
}
