using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputTypeListInfo
    {
        public string Title { get; set; }
        public string InputTypeCode { get; set; }
        public IList<InputTypeViewModelList> Views { get; set; }
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
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public string ReferenceCategoryTitleFieldName { get; set; }
        public EnumDataType DataTypeId { get; set; }

    }

    public class InputValueBillListOutput
    {
        public long InputValueBillId { get; set; }
        public IDictionary<int, string> FieldValues { get; set; }
    }
}
