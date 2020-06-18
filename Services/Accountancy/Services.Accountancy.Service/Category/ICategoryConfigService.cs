﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Model.Category;

namespace VErp.Services.Accountancy.Service.Category
{
    public interface ICategoryDataService
    {
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, string filters, int page, int size);

        Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(int categoryId, int fId);

        Task<ServiceResult<int>> AddCategoryRow(int categoryId, NonCamelCaseDictionary data);

    }
}
