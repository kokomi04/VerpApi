﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IRequestOutsourcePartService
    {
        Task<PageData<RequestOutsourcePartDetailInfo>> GetListRequestOutsourcePart(string keyWord, int page, int size, Clause filters = null);
        Task<RequestOutsourcePartInfo> GetRequestOutsourcePartExtraInfo(int requestOutsourcePartId = 0);
        Task<long> CreateRequestOutsourcePart(RequestOutsourcePartInfo req);
        Task<bool> UpdateRequestOutsourcePart(int requestOutsourcePartId, RequestOutsourcePartInfo req);
        Task<bool> DeletedRequestOutsourcePart(int requestOutsourcePart);

    }
}
