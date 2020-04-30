using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputTypeListInfo
    {
        public string Title { get; set; }
        public string InputTypeCode { get; set; }
        public IList<InputTypeListColumn> ColumnsInList { get; set; }
    }

    public class InputTypeListColumn
    {
        public int InputAreaFieldId { get; set; }
        public int InputAreaId { get; set; }
        public int FieldIndex { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public bool IsMultiRow { get; set; }
    }

    public class InputValueBillListOutput
    {
        public long InputValueBillId { get; set; }
        public IDictionary<int, string> FieldValues { get; set; }
    }
}
