﻿using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Manafacturing.Model.ProductionProcess
{
    public class ProductionProcessWarningMessage
    {
        public EnumProductionProcessWarningCode WarningCode { get; set; }
        public string GroupName { get; set; }
        public long? ObjectId { get; set; }
        public string ObjectCode { get; set; }
        public string Message { get; set; }
    }
}
