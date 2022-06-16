using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Guide;

namespace VErp.Services.Master.Service.Guide
{
    public interface IGuideCateService
    {
        Task<int> Create(GuideCateModel model);
        Task<bool> Delete(int guideCateId);
        Task<IList<GuideCateModel>> GetList();
        Task<GuideCateModel> Info(int guideCateId);
        Task<bool> Update(int guideCateId, GuideCateModel model);
    }
}
