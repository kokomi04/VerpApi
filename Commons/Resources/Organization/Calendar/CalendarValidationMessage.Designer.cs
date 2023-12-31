﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Organization.Calendar {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class CalendarValidationMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CalendarValidationMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Organization.Calendar.CalendarValidationMessage", typeof(CalendarValidationMessage).Assembly);
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
        ///   Looks up a localized string similar to Mã lịch làm việc đã tồn tại.
        /// </summary>
        public static string CalendarCodeAlreadyExists {
            get {
                return ResourceManager.GetString("CalendarCodeAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lịch làm việc không tồn tại.
        /// </summary>
        public static string CalendarDoesNotExists {
            get {
                return ResourceManager.GetString("CalendarDoesNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tên lịch làm việc đã tồn tại.
        /// </summary>
        public static string CalendarNameAlreadyExists {
            get {
                return ResourceManager.GetString("CalendarNameAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đã tồn tại thay đổi lịch làm việc vào ngày {0:dd/MM/yyyy}.
        /// </summary>
        public static string CalendarStartDateAlreadyExists {
            get {
                return ResourceManager.GetString("CalendarStartDateAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không tồn tại thay đổi lịch làm việc vào ngày {0:dd/MM/yyyy}.
        /// </summary>
        public static string CalendarStartDateInvalid {
            get {
                return ResourceManager.GetString("CalendarStartDateInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ngày nghỉ không tồn tại.
        /// </summary>
        public static string DayOffDoesNotExist {
            get {
                return ResourceManager.GetString("DayOffDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mã lịch không được để trống.
        /// </summary>
        public static string EmptyCalendarCode {
            get {
                return ResourceManager.GetString("EmptyCalendarCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Thông tin làm việc trong tuần chưa đủ.
        /// </summary>
        public static string MissingDaysOfWeek {
            get {
                return ResourceManager.GetString("MissingDaysOfWeek", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vui lòng chọn ngày hiệu lực.
        /// </summary>
        public static string MissingStartDate {
            get {
                return ResourceManager.GetString("MissingStartDate", resourceCulture);
            }
        }
    }
}
