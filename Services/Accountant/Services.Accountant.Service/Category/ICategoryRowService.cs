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
        Task<PageData<CategoryRowListOutputModel>> GetCategoryRows(int categoryId, string keyword, Clause filters, int page, int size);

        Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId);

        Task<ServiceResult<List<MapTitleOutputModel>>> MapTitle(MapTitleInputModel[] categoryValues);

        Task<ServiceResult<int>> AddCategoryRow(int categoryId, CategoryRowInputModel data);

        Task<Enum> UpdateCategoryRow(int categoryId, int categoryRowId, CategoryRowInputModel data);

        Task<Enum> DeleteCategoryRow(int categoryId, int categoryRowId);

        Task<ServiceResult> ImportCategoryRow(int categoryId, Stream stream);

        Task<ServiceResult<MemoryStream>> GetImportTemplateCategory(int categoryId);

        Task<ServiceResult<MemoryStream>> ExportCategory(int categoryId);
    }
}
