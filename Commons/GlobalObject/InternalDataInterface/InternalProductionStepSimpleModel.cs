﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InternalProductionStepSimpleModel
    {
        public long? ProductionStepId { get; set; }
        public string ProductionStepCode { get; set; }
        public string Title { get; set; }
        public string OutputString { get; set; }
        public int? StepId { get; set; }
    }
}
