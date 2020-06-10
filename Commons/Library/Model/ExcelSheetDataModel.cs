using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Model
{
    public class ExcelSheetDataModel
    {
        public string SheetName { get; set; }
        public NonCamelCaseDictionary[] Rows { get; set; }
    }
}
