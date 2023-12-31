﻿using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.CategoryConfig
{
    public class OperatorModel : LogicOperatorModel
    {
        public int ParamNumber { get; set; }
        public EnumDataType[] AllowedDataType { get; set; }
    }

    public class LogicOperatorModel: ValueTitleModel
    {
       
    }

    public class ValueTitleModel
    {
        public int Value { get; set; }
        public string Title { get; set; }
    }
}
