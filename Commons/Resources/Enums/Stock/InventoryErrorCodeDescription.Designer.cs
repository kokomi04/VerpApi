﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Enums.Stock {
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
    public class InventoryErrorCodeDescription {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InventoryErrorCodeDescription() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Enums.Stock.InventoryErrorCodeDescription", typeof(InventoryErrorCodeDescription).Assembly);
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
        ///   Looks up a localized string similar to Không được phép thay đổi thông tin sản phẩm của phiếu nhập/xuất được tạo từ yêu cầu.
        /// </summary>
        public static string CanNotChangeProductInventoryHasRequirement {
            get {
                return ResourceManager.GetString("CanNotChangeProductInventoryHasRequirement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không thể thay đổi kho ở phiếu nhập/xuất.
        /// </summary>
        public static string CanNotChangeStock {
            get {
                return ResourceManager.GetString("CanNotChangeStock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bạn cần cập nhật dữ liệu hợp lệ cho kiện/phiếu xuất (đầu ra &lt;= đầu vào).
        /// </summary>
        public static string InOuputAffectObjectsInvalid {
            get {
                return ResourceManager.GetString("InOuputAffectObjectsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Kiện hàng không hợp lệ (sản phẩm hoặc đơn vị lưu không trùng khớp).
        /// </summary>
        public static string InvalidPackage {
            get {
                return ResourceManager.GetString("InvalidPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phiếu đã được duyệt.
        /// </summary>
        public static string InventoryAlreadyApproved {
            get {
                return ResourceManager.GetString("InventoryAlreadyApproved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Thông tin phiếu đã tồn tại.
        /// </summary>
        public static string InventoryAlreadyExisted {
            get {
                return ResourceManager.GetString("InventoryAlreadyExisted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mã phiếu &quot;{0}&quot; đã tồn tại.
        /// </summary>
        public static string InventoryCodeAlreadyExisted {
            get {
                return ResourceManager.GetString("InventoryCodeAlreadyExisted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mã phiếu không có.
        /// </summary>
        public static string InventoryCodeEmpty {
            get {
                return ResourceManager.GetString("InventoryCodeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phiếu chưa được duyệt.
        /// </summary>
        public static string InventoryNotApprovedYet {
            get {
                return ResourceManager.GetString("InventoryNotApprovedYet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không tìm thấy phiếu xuất/nhập kho.
        /// </summary>
        public static string InventoryNotFound {
            get {
                return ResourceManager.GetString("InventoryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Kiện không đủ số lượng để xuất kho.
        /// </summary>
        public static string NotEnoughQuantity {
            get {
                return ResourceManager.GetString("NotEnoughQuantity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tính năng này chưa được hỗ trợ.
        /// </summary>
        public static string NotSupportedYet {
            get {
                return ResourceManager.GetString("NotSupportedYet", resourceCulture);
            }
        }
    }
}
