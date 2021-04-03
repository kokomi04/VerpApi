using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.DynamicBill
{
    public class TypeGlobalSetting
    {
        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public string AfterInsertLinesJsAction { get; set; }
    }
}
