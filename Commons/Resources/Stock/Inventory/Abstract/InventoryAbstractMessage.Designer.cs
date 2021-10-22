﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Stock.Inventory.Abstract {
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
    public class InventoryAbstractMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InventoryAbstractMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Stock.Inventory.Abstract.InventoryAbstractMessage", typeof(InventoryAbstractMessage).Assembly);
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
        ///   Looks up a localized string similar to Ngày chứng từ không được phép trước ngày chốt sổ.
        /// </summary>
        public static string BillDateLocked {
            get {
                return ResourceManager.GetString("BillDateLocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Số lượng {0} trong kho tại thời điểm {1} phiếu  {2} không đủ. Số tồn là {3} không hợp lệ.
        /// </summary>
        public static string BillDetailError {
            get {
                return ResourceManager.GetString("BillDetailError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Có phiếu bị lỗi. Không thể lấy thông tin chi tiết lỗi.
        /// </summary>
        public static string BillDetailErrorUnknown {
            get {
                return ResourceManager.GetString("BillDetailErrorUnknown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nhập kho.
        /// </summary>
        public static string InventoryInput {
            get {
                return ResourceManager.GetString("InventoryInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Xuất kho.
        /// </summary>
        public static string InventoryOuput {
            get {
                return ResourceManager.GetString("InventoryOuput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lỗi cập nhật trạng thái lệnh sản xuất {0}.
        /// </summary>
        public static string UpdateProductionOrderStatusError {
            get {
                return ResourceManager.GetString("UpdateProductionOrderStatusError", resourceCulture);
            }
        }
    }
}