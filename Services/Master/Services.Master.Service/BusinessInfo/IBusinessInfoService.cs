using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.BusinessInfo;

namespace VErp.Services.Master.Service.BusinessInfo
{
    public interface IBusinessInfoService
    {
        Task<ApiResponse<BusinessInfoModel>> GetBusinessInfo();
        Task<Enum> UpdateBusinessInfo(BusinessInfoModel data);
    }
}
