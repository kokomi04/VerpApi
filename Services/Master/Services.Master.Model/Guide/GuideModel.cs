﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using GuideEnity = VErp.Infrastructure.EF.MasterDB.Guide;

namespace VErp.Services.Master.Model.Guide
{
    public class GuideModel : GuideModelOutput
    {
        public string Description { get; set; }
    }

    public class GuideModelOutput: IMapFrom<GuideEnity>
    {
        public int GuideId { get; set; }
        public int? GuideCateId { get; set; }
        public string GuideCode { get; set; }
        public string Title { get; set; }
        public int SortOrder { get; set; }
    }
}
