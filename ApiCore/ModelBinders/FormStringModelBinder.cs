using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ApiCore.ModelBinders
{
    public class FormStringModelBinder : IModelBinder
    {
        private Type modelType;
        private string modelName;
        public FormStringModelBinder(Type type, string modelName)
        {
            modelType = type;
            this.modelName = modelName;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            //string valueFromBody = string.Empty;

            //using (var sr = new StreamReader(bindingContext.HttpContext.Request.Body))
            //{
            //    valueFromBody = sr.ReadToEnd();
            //}

            if (!bindingContext.HttpContext.Request.Form.ContainsKey(modelName))
                return Task.CompletedTask;

            var data = bindingContext.HttpContext.Request.Form[modelName];
            if (string.IsNullOrEmpty(data))
            {
                return Task.CompletedTask;
            }

            var modelValue = data.ToString().JsonDeserialize(modelType);
            bindingContext.Result = ModelBindingResult.Success(modelValue);
            return Task.CompletedTask;
        }
    }
}
