using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig
{
    public interface IPrintConfigHeaderService<TModel, TViewModel>
    {
        Task<TModel> GetHeaderById(int headerId);
        Task<PageData<TViewModel>> Search(string keyword, int page, int size);
        Task<int> CreateHeader(TModel model);
        Task<bool> UpdateHeader(int headerId, TModel model);
        Task<bool> DeleteHeader(int headerId);
    }

}
