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

        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(string categoryCode, string keyword, string filters, int page, int size);

        Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(int categoryId, int fId);

        Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(string categoryCode, int fId);

        Task<ServiceResult<int>> AddCategoryRow(int categoryId, Dictionary<string, string> data);

        Task<int> UpdateCategoryRow(int categoryId, int fId, Dictionary<string, string> data);

        Task<int> DeleteCategoryRow(int categoryId, int fId);
    }
}
