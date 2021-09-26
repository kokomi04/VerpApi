using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.DraftData;

namespace VErp.Services.Manafacturing.Service.DraftData
{
    public interface IDraftDataService
    {
      
        Task<DraftDataModel> UpdateDraftData(DraftDataModel data);
        Task<DraftDataModel> GetDraftData(int objectTypeId, long objectId);
        Task<bool> DeleteDraftData(int objectTypeId, long objectId);
    }
}
