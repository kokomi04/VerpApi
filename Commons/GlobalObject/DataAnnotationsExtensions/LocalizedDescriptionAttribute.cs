using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.GlobalObject.DataAnnotationsExtensions
{

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter)]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
      
        public string ResourceName { get; set; }
    
        public Type ResourceType { get; set; }

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
