﻿using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigCustomModel : PrintConfigBaseModel, IMapFrom<PrintConfigCustom>
    {
        public int? PrintConfigCustomId { get; set; }
        public int? PrintConfigStandardId { get; set; }
        public int SubsidiaryId { get; set; }
        public int PrintConfigHeaderId { get; set; }
    }
}
