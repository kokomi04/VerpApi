﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.PurchaseOrder.PurchasingRequest {
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
    public class PurchasingRequestMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PurchasingRequestMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.PurchaseOrder.PurchasingRequest.PurchasingRequestMessage", typeof(PurchasingRequestMessage).Assembly);
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
        ///   Looks up a localized string similar to Không thể xóa yêu cầu vật tư khi đang tồn tại đề nghị vật tư được liên kết {0}.
        /// </summary>
        public static string CanNotDeletePurchasingRequestWithExistedSuggest {
            get {
                return ResourceManager.GetString("CanNotDeletePurchasingRequestWithExistedSuggest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lỗi tính toán số lượng mặt hàng &quot;{0}&quot;.
        /// </summary>
        public static string ErrorCalcQuantity {
            get {
                return ResourceManager.GetString("ErrorCalcQuantity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tìm thấy {0} mặt hàng &quot;{1}&quot;.
        /// </summary>
        public static string FoundNumberProduct {
            get {
                return ResourceManager.GetString("FoundNumberProduct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tìm thấy {0} đơn vị chuyển đổi &quot;{1}&quot; mặt hàng &quot;{2}&quot;.
        /// </summary>
        public static string FoundNumberPuOfProduct {
            get {
                return ResourceManager.GetString("FoundNumberPuOfProduct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Số lượng không hợp lệ mặt hàng &quot;{0}&quot;.
        /// </summary>
        public static string InvalidQuantity {
            get {
                return ResourceManager.GetString("InvalidQuantity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không tìm thấy mặt hàng &quot;{0}&quot;.
        /// </summary>
        public static string NoProductFound {
            get {
                return ResourceManager.GetString("NoProductFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đơn vị chuyển đổi &quot;{0}&quot; của mặt hàng &quot;{1}&quot; không tìm thấy.
        /// </summary>
        public static string NoPuOfProductFound {
            get {
                return ResourceManager.GetString("NoPuOfProductFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đơn vị chuyển đổi không thuộc về mặt hàng.
        /// </summary>
        public static string PuConversionDoesNotBelongToProduct {
            get {
                return ResourceManager.GetString("PuConversionDoesNotBelongToProduct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không tìm thấy đơn vị tính của mặt hàng &quot;{0}&quot;.
        /// </summary>
        public static string PuDefaultError {
            get {
                return ResourceManager.GetString("PuDefaultError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không thể sửa YCVT từ {0}.
        /// </summary>
        public static string PurchasingRequestPreventFrom {
            get {
                return ResourceManager.GetString("PurchasingRequestPreventFrom", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Số lượng không hợp lệ.
        /// </summary>
        public static string QuantityInvalid {
            get {
                return ResourceManager.GetString("QuantityInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Yêu cầu nhập số lượng.
        /// </summary>
        public static string QuantityRequired {
            get {
                return ResourceManager.GetString("QuantityRequired", resourceCulture);
            }
        }
    }
}