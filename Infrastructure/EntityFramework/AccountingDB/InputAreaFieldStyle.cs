using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputAreaFieldStyle 
    {
        public InputAreaFieldStyle()
        {
        }
        public int InputAreaFieldId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string TitleStyleJson { get; set; }
        public string InputStyleJson { get; set; }
        public string OnFocus { get; set; }
        public string OnKeydown { get; set; }
        public string OnKeypress { get; set; }
        public string OnBlur { get; set; }
        public string OnChange { get; set; }
        public bool AutoFocus { get; set; }
        public int Column { get; set; }


        public virtual InputAreaField InputAreaField { get; set; }
    }
}
