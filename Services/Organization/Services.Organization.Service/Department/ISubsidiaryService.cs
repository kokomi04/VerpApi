using Services.Organization.Model.Deparment;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;

namespace Services.Organization.Service.Department
{
    public interface ISubsidiaryService
    {
        Task<PageData<SubsidiaryOutput>> GetList(string keyword, int page, int size);

        Task<ServiceResult<int>> Create(SubsidiaryModel data);

        Task<ServiceResult> Update(int subsidiaryId, SubsidiaryModel data);

        Task<ServiceResult<SubsidiaryModel>> GetInfo(int subsidiaryId);

        Task<ServiceResult> Delete(int subsidiaryId);
    }
}
