using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Users
{
    public class UserImportModel
    {
        [Display(Name = "Mã Nhân viên", GroupName = "TT cơ bản")]
        public string EmployeeCode { get; set; }

        [Display(Name = "Họ và tên", GroupName = "TT cơ bản")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Email", GroupName = "TT cơ bản")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại", GroupName = "TT cơ bản")]
        public string Phone { get; set; }

        [Display(Name = "Địa chỉ", GroupName = "TT cơ bản")]
        public string Address { get; set; }

        [Display(Name = "Giới tính", GroupName = "TT cơ bản")]
        public EnumGender? GenderId { get; set; }

        [Display(Name = "Tên đăng nhập", GroupName = "TT cơ bản")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu", GroupName = "TT cơ bản")]
        public string Password { get; set; }

        [Display(Name = "Trạng thái", GroupName = "TT cơ bản")]
        public EnumUserStatus UserStatusId { get; set; }

        [Display(Name = "Nhóm quyền", GroupName = "TT cơ bản")]
        public int? RoleId { get; set; }

        //1
        [Display(Name = "TT Bộ phận 1", GroupName = "Bộ phận 1")]
        public UserImportDepartmentModel Department1 { get; set; }

        [Display(Name = "Ngày vào Bộ phận 1", GroupName = "Bộ phận 1")]
        public DateTime? EffectiveDate1 { get; set; }
        [Display(Name = "Ngày kết thúc làm việc tại bộ phận 1", GroupName = "Bộ phận 1")]
        public DateTime? ExpirationDate1 { get; set; }


        //2
        [Display(Name = "TT Bộ phận 2", GroupName = "Bộ phận 2")]
        public UserImportDepartmentModel Department2 { get; set; }

        [Display(Name = "Ngày vào Bộ phận 2", GroupName = "Bộ phận 2")]
        public DateTime? EffectiveDate2 { get; set; }
        [Display(Name = "Ngày kết thúc làm việc tại bộ phận 2", GroupName = "Bộ phận 2")]
        public DateTime? ExpirationDate2 { get; set; }


        //3
        [Display(Name = "TT Bộ phận 3", GroupName = "Bộ phận 3")]
        public UserImportDepartmentModel Department3 { get; set; }

        [Display(Name = "Ngày vào Bộ phận 3", GroupName = "Bộ phận 3")]
        public DateTime? EffectiveDate3 { get; set; }
        [Display(Name = "Ngày kết thúc làm việc tại bộ phận 3", GroupName = "Bộ phận 3")]
        public DateTime? ExpirationDate3 { get; set; }

        //4
        [Display(Name = "TT Bộ phận 4", GroupName = "Bộ phận 4")]
        public UserImportDepartmentModel Department4 { get; set; }

        [Display(Name = "Ngày vào Bộ phận 4", GroupName = "Bộ phận 4")]
        public DateTime? EffectiveDate4 { get; set; }
        [Display(Name = "Ngày kết thúc làm việc tại bộ phận 4", GroupName = "Bộ phận 4")]
        public DateTime? ExpirationDate4 { get; set; }

        //5
        [Display(Name = "TT Bộ phận 5", GroupName = "Bộ phận 5")]
        public UserImportDepartmentModel Department5 { get; set; }

        [Display(Name = "Ngày vào Bộ phận 5", GroupName = "Bộ phận 5")]
        public DateTime? EffectiveDate5 { get; set; }
        [Display(Name = "Ngày kết thúc làm việc tại bộ phận 5", GroupName = "Bộ phận 5")]
        public DateTime? ExpirationDate5 { get; set; }

    }

    public class UserImportDepartmentModel
    {
        [Display(Name = "ID Bộ phận")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Mã Bộ phận")]
        public string DepartmentCode { get; set; }

        [Display(Name = "Tên Bộ phận")]
        public string DepartmentName { get; set; }
    }
}
