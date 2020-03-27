using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.BusinessInfo;

namespace VErp.Services.Organization.Service.BusinessInfo
{
    public interface IBusinessInfoService
    {
        Task<ServiceResult<BusinessInfoModel>> GetBusinessInfo();
        Task<Enum> UpdateBusinessInfo(int updatedUserId, BusinessInfoModel data);
    }
}
