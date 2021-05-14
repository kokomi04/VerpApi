﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigStandardModel: PrintConfigBaseModel, IMapFrom<PrintConfigStandard>
    {
        public int? PrintConfigStandardId { get; set; }
    }
}
