﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Guide;

namespace VErp.Services.Master.Service.Guide
{
    public interface IGuideService
    {
        Task<List<GuideModel>> GetList();
        Task<bool> Update(int guideId, GuideModel model);
        Task<int> Create(GuideModel model);
        Task<bool> Deleted(int guideId);
    }
}
