using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using GuideEnity = VErp.Infrastructure.EF.MasterDB.Guide;

namespace VErp.Services.Master.Model.Guide
{
    public class GuideModel : IMapFrom<GuideEnity>
    {
        public int GuideId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
