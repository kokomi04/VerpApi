﻿using Services.Organization.Model.Deparment;
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

        Task<int> Create(SubsidiaryModel data);

        Task<bool> Update(int subsidiaryId, SubsidiaryModel data);

        Task<SubsidiaryModel> GetInfo(int subsidiaryId);

        Task<bool> Delete(int subsidiaryId);
    }
}
