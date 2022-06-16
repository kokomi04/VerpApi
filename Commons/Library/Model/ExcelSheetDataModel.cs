﻿using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Model
{
    public class ExcelSheetDataModel
    {
        public string SheetName { get; set; }
        public NonCamelCaseDictionary<string>[] Rows { get; set; }
    }
}
