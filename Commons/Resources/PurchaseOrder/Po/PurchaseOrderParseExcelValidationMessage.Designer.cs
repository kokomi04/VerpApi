﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.PurchaseOrder.Po {
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
    public class PurchaseOrderParseExcelValidationMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PurchaseOrderParseExcelValidationMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.PurchaseOrder.Po.PurchaseOrderParseExcelValidationMessage", typeof(PurchaseOrderParseExcelValidationMessage).Assembly);
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
        ///   Looks up a localized string similar to Tìm thấy {0} mặt hàng {1}.
        /// </summary>
        public static string FoundNumberOfProduct {
            get {
                return ResourceManager.GetString("FoundNumberOfProduct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tìm thấy {0} đơn vị chuyển đổi {1} mặt hàng {2}.
        /// </summary>
        public static string FoundNumberOfPuConversion {
            get {
                return ResourceManager.GetString("FoundNumberOfPuConversion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đơn vị tính của mặt hàng {0} không tìm thấy.
        /// </summary>
        public static string PrimaryPuOfProductNotFound {
            get {
                return ResourceManager.GetString("PrimaryPuOfProductNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không tìm thấy mặt hàng {0}.
        /// </summary>
        public static string ProductInfoNotFound {
            get {
                return ResourceManager.GetString("ProductInfoNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không tìm thấy đơn vị chuyển đổi {0} mặt hàng {1}.
        /// </summary>
        public static string PuOfProductNotFound {
            get {
                return ResourceManager.GetString("PuOfProductNotFound", resourceCulture);
            }
        }
    }
}
