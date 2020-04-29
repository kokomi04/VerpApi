using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryRowService
    {
        Task<PageData<CategoryRowListOutputModel>> GetCategoryRows(int categoryId, string keyword, FilterModel[] filters, int page, int size);

        Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId);

        Task<ServiceResult<int>> AddCategoryRow(int updatedUserId, int categoryId, CategoryRowInputModel data);

        Task<Enum> UpdateCategoryRow(int updatedUserId, int categoryId, int categoryRowId, CategoryRowInputModel data);

        Task<Enum> DeleteCategoryRow(int updatedUserId, int categoryId, int categoryRowId);

        Task<ServiceResult> ImportCategoryRow(int updatedUserId, int categoryId, Stream stream);

        Task<ServiceResult<MemoryStream>> GetImportTemplateCategory(int categoryId);

        Task<ServiceResult<MemoryStream>> ExportCategory(int categoryId);
    }
}
