using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.ApiCore.ModelBinders
{

    public class FromFormStringAttribute : Attribute, IBindingSourceMetadata
    {

        public FromFormStringAttribute()
        {
            BindingSource = new FormStringBindingSource(BindingSource.Form.Id, BindingSource.Form.DisplayName, BindingSource.Form.IsGreedy, BindingSource.Form.IsFromRequest);
        }
        public BindingSource BindingSource { get; private set; }


    }

    internal class FormStringBindingSource : BindingSource
    {
        public FormStringBindingSource(string id, string displayName, bool isGreedy, bool isFromRequest) : base(id, displayName, isGreedy, isFromRequest)
        {

        }
    }
}
