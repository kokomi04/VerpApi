using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Organization.Model.Department;

namespace VErp.Services.Organization.Model.Employee
{
    public class EmployeeModel
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public EnumGender? GenderId { get; set; }
        public long? AvatarFileId { get; set; }
        public int DepartmentId { get; set; }
        public DepartmentModel Department { get; set; }
    }
}
