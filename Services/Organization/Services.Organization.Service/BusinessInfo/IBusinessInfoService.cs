using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Organization;

namespace VErp.Services.Organization.Service.BusinessInfo
{
    public interface IBusinessInfoService
    {
        Task<BusinessInfoModel> GetBusinessInfo();
        Task<bool> UpdateBusinessInfo(int updatedUserId, BusinessInfoModel data);
    }
}
