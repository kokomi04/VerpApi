﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Enums.System {
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
    public class GeneralCodeDescription {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal GeneralCodeDescription() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Enums.System.GeneralCodeDescription", typeof(GeneralCodeDescription).Assembly);
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
        ///   Looks up a localized string similar to Đang có tranh chấp tài nguyên bởi xử lý khác, vui lòng thử lại sau ít phút.
        /// </summary>
        public static string DistributedLockExeption {
            get {
                return ResourceManager.GetString("DistributedLockExeption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bạn không có quyền thực hiện chức năng này.
        /// </summary>
        public static string Forbidden {
            get {
                return ResourceManager.GetString("Forbidden", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lỗi hệ thống.
        /// </summary>
        public static string InternalError {
            get {
                return ResourceManager.GetString("InternalError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tham số không hợp lệ.
        /// </summary>
        public static string InvalidParams {
            get {
                return ResourceManager.GetString("InvalidParams", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mã item đã tồn tại, vui lòng chọn mã khác!.
        /// </summary>
        public static string ItemCodeExisted {
            get {
                return ResourceManager.GetString("ItemCodeExisted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Item không tồn tại.
        /// </summary>
        public static string ItemNotFound {
            get {
                return ResourceManager.GetString("ItemNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tài khoản không có quyền truy cập vào hệ thống.
        /// </summary>
        public static string LockedOut {
            get {
                return ResourceManager.GetString("LockedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không thực hiện được. Chức năng này tạm thời chưa được hỗ trợ.
        /// </summary>
        public static string NotYetSupported {
            get {
                return ResourceManager.GetString("NotYetSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Thành công.
        /// </summary>
        public static string Success {
            get {
                return ResourceManager.GetString("Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to X-Module header was not found.
        /// </summary>
        public static string X_ModuleMissing {
            get {
                return ResourceManager.GetString("X_ModuleMissing", resourceCulture);
            }
        }
    }
}
