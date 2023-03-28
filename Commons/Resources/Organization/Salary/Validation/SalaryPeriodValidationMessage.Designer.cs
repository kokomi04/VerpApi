﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Organization.Salary.Validation {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class SalaryPeriodValidationMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SalaryPeriodValidationMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Organization.Salary.Validation.SalaryPeriodValidationMessage", typeof(SalaryPeriodValidationMessage).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dữ liệu cập nhật không đủ số lượng nhân viên của nhóm bảng lương.
        /// </summary>
        public static string DiffNumberOfUpdatedEmployeeSalary {
            get {
                return ResourceManager.GetString("DiffNumberOfUpdatedEmployeeSalary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dữ liệu nhân viên không hợp lệ! ID trùng nhau ({0}).
        /// </summary>
        public static string EmployeeIDDuplicated {
            get {
                return ResourceManager.GetString("EmployeeIDDuplicated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dữ liệu nhân viên không hợp lệ! ID = 0.
        /// </summary>
        public static string EmployeeIDZero {
            get {
                return ResourceManager.GetString("EmployeeIDZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dữ liệu nhân viên không được phép!  (ID: {0}).
        /// </summary>
        public static string EmployeeNotFoundInEval {
            get {
                return ResourceManager.GetString("EmployeeNotFoundInEval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dữ liệu trường {0}: {1} không được phép chỉnh sửa ({2}).
        /// </summary>
        public static string FieldIsNotEditable {
            get {
                return ResourceManager.GetString("FieldIsNotEditable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Trạng thái không hợp lệ.
        /// </summary>
        public static string InvalidStatus {
            get {
                return ResourceManager.GetString("InvalidStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Kỳ lương chỉ được phép Duyệt/kiểm tra khi tất cả các bảng lương đã được duyệt!.
        /// </summary>
        public static string PeriodGroupHasNotApprovedYet {
            get {
                return ResourceManager.GetString("PeriodGroupHasNotApprovedYet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cần phải tạo bảng lương cho các Nhóm bảng lương trong kỳ!.
        /// </summary>
        public static string PeriodGroupHasNotCreatedYet {
            get {
                return ResourceManager.GetString("PeriodGroupHasNotCreatedYet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Kỳ tính lương tháng {0} năm {1} đã được tạo.
        /// </summary>
        public static string PeriodHasBeenCreated {
            get {
                return ResourceManager.GetString("PeriodHasBeenCreated", resourceCulture);
            }
        }
    }
}
