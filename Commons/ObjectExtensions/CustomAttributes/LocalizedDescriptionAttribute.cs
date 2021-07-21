using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.ObjectExtensions.CustomAttributes
{

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter)]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        //
        // Summary:
        //     Gets or sets the error message resource name to use in order to look up the System.ComponentModel.DataAnnotations.ValidationAttribute.ErrorMessageResourceType
        //     property value if validation fails.
        //
        // Returns:
        //     The error message resource that is associated with a validation control.
        public string ResourceName { get; set; }
        //
        // Summary:
        //     Gets or sets the resource type to use for error-message lookup if validation
        //     fails.
        //
        // Returns:
        //     The type of error message that is associated with a validation control.
        public Type ResourceType { get; set; }

        public LocalizedDescriptionAttribute()
        {

        }

        public LocalizedDescriptionAttribute(Type resourceType)
        {
            ResourceType = resourceType;
        }

        public override string Description
        {
            get
            {
                System.Resources.ResourceManager rs = new System.Resources.ResourceManager(ResourceType);
                return rs.GetString(ResourceName);
            }
        }
    }
}
