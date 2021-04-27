using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Infrastructure.ApiCore.ModelBinders
{
    public class CustomModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //if (context.Metadata.proper)
            //{
            //   // return new BinderTypeModelBinder(typeof(ExcelMappingModelBinder));
            //    return new ExcelMappingModelBinder(context.Metadata.ModelType);
            //}
            //if (context.Metadata.Name == "mapping")
            //{
            //    var a = 1;
            //}
            
            if (context.Metadata?.BindingSource?.GetType() == typeof(FormStringBindingSource))
            {
                return new FormStringModelBinder(context.Metadata.ModelType, context.Metadata.Name);
            }
            return null;
        }
    }
}
