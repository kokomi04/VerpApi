﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Enums.ErrorCodes.Product {
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
    public class ProductTypeErrorCodeDescription {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ProductTypeErrorCodeDescription() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Enums.ErrorCodes.Product.ProductTypeErrorCodeDescription", typeof(ProductTypeErrorCodeDescription).Assembly);
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
        ///   Looks up a localized string similar to Để xóa loại cha, vui lòng xóa hoặc di chuyển loại con.
        /// </summary>
        public static string CanNotDeletedParentProductType {
            get {
                return ResourceManager.GetString("CanNotDeletedParentProductType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tên loại mã không được bỏ trống.
        /// </summary>
        public static string EmptyProductTypeName {
            get {
                return ResourceManager.GetString("EmptyProductTypeName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loại cha không tồn tại.
        /// </summary>
        public static string ParentProductTypeNotfound {
            get {
                return ResourceManager.GetString("ParentProductTypeNotfound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không thể xóa loại mã hàng đang được sử dụng.
        /// </summary>
        public static string ProductTypeInUsed {
            get {
                return ResourceManager.GetString("ProductTypeInUsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tên loại mã mặt hàng đã tồn tại, vui lòng chọn tên khác.
        /// </summary>
        public static string ProductTypeNameAlreadyExisted {
            get {
                return ResourceManager.GetString("ProductTypeNameAlreadyExisted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loại mã mặt hàng không tồn tại.
        /// </summary>
        public static string ProductTypeNotfound {
            get {
                return ResourceManager.GetString("ProductTypeNotfound", resourceCulture);
            }
        }
    }
}
