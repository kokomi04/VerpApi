using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Guide;

namespace VErp.Services.Master.Service.Guide
{
    public interface IGuideService
    {
        Task<PageData<GuideModel>> GetList(string keyword, int page, int size);
        Task<GuideModel> GetGuideById(int guideId);
        Task<IList<GuideModel>> GetGuidesByCode(string guideCode);
        Task<bool> Update(int guideId, GuideModel model);
        Task<int> Create(GuideModel model);
        Task<bool> Deleted(int guideId);
    }
}
