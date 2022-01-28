using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Dictionary;

namespace VErp.Services.Master.Service.Dictionay
{
    public interface IReuseContentService
    {
        Task<IList<ReuseContentModel>> GetList(string key);
        Task<long> Create(ReuseContentModel model);
        Task<bool> Delete(long reuseContentId);
        Task<ReuseContentModel> Info(long reuseContentId);
        Task<bool> Update(long reuseContentId, ReuseContentModel model);
    }
}
