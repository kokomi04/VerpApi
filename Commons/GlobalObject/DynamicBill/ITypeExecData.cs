using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VErp.Commons.Constants;

namespace VErp.Commons.GlobalObject.DynamicBill
{
    public interface ITypeData
    {
        string PreLoadAction { get; set; }
        string PostLoadAction { get; set; }
        string AfterLoadAction { get; set; }
        string BeforeSubmitAction { get; set; }
        string BeforeSaveAction { get; set; }
        string AfterSaveAction { get; set; }
        string AfterUpdateRowsJsAction { get; set; }
    }

    public interface IFieldData
    {
        string OnFocus { get; set; }
        string OnKeydown { get; set; }
        string OnKeypress { get; set; }
        string OnBlur { get; set; }
        string OnChange { get; set; }
        string OnClick { get; set; }
        string ReferenceUrl { get; set; }
    }



    public interface ITypeExecData
    {
        string Title { get; }

        string PreLoadActionExec { get; }
        string PostLoadActionExec { get; }
        string AfterLoadActionExec { get; }
        string BeforeSubmitActionExec { get; }
        string BeforeSaveActionExec { get; }
        string AfterSaveActionExec { get; }
        string AfterUpdateRowsJsActionExec { get; }
    }


    public interface IFieldExecData
    {
        string OnFocusExec { get; }
        string OnKeydownExec { get; }
        string OnKeypressExec { get; }
        string OnBlurExec { get; }
        string OnChangeExec { get; }
        string OnClickExec { get; }

        string ReferenceUrlExec { get; }
    }


    public class ExecCodeCombine<T>
    {
        private T typeData;

        private IDictionary<string, PropertyInfo> dataProperties;
        public ExecCodeCombine(T typeData)
        {
            this.typeData = typeData;
            dataProperties = typeof(T).GetProperties().ToDictionary(p => p.Name, p => p);
        }

        public string GetExecCode(string propName, T globalSetting)
        {
            var prop = dataProperties[propName];

            var globalValue = prop.GetValue(globalSetting) as string;

            var customValue = prop.GetValue(typeData) as string;
            if (string.IsNullOrWhiteSpace(customValue)) return globalValue;

            return customValue.Replace(ActionCodeConstants.SUPER, globalValue);
        }

    }
}
