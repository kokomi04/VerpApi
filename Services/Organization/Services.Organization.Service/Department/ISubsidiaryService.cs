﻿using Services.Organization.Model.Deparment;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;

namespace Services.Organization.Service.Department
{
    public interface ISubsidiaryService
    {
        Task<PageData<SubsidiaryOutput>> GetList(string keyword, int page, int size, Clause filters = null);

        Task<int> Create(SubsidiaryModel data);

        Task<bool> Update(int subsidiaryId, SubsidiaryModel data);

        Task<SubsidiaryOutput> GetInfo(int subsidiaryId);

        Task<bool> Delete(int subsidiaryId);

        Task<IList<SubsidiaryOutput>> GetList();
    }
}
