﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verp.Resources.Stock.InventoryProcess {
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
    public class InventoryBillOutputMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InventoryBillOutputMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Verp.Resources.Stock.InventoryProcess.InventoryBillOutputMessage", typeof(InventoryBillOutputMessage).Assembly);
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
        ///   Looks up a localized string similar to Số dư trong kiện &quot;{0}&quot; mặt hàng &quot;{1}&quot; ({2}) &lt;  {3} không đủ để xuất.
        /// </summary>
        public static string NotEnoughBalanceInPackage {
            get {
                return ResourceManager.GetString("NotEnoughBalanceInPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Số dư trong kho mặt hàng &quot;{0}&quot; ({1}) không đủ để xuất {2}.
        /// </summary>
        public static string NotEnoughBalanceInStock {
            get {
                return ResourceManager.GetString("NotEnoughBalanceInStock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Không có số dư trong kiện.
        /// </summary>
        public static string NotEnoughBalancePackageQuantityZero {
            get {
                return ResourceManager.GetString("NotEnoughBalancePackageQuantityZero", resourceCulture);
            }
        }
    }
}