using System.Threading.Tasks;
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
