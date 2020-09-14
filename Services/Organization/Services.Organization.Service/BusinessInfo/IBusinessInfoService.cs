using System;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Services.Organization.Service.BusinessInfo
{
    public interface IBusinessInfoService
    {
        Task<BusinessInfoModel> GetBusinessInfo();
        Task<bool> UpdateBusinessInfo(int updatedUserId, BusinessInfoModel data);
    }
}
