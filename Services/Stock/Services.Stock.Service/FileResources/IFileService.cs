using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;
using FileEnity = VErp.Infrastructure.EF.StockDB.File;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileService
    {
        Task<long> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file);
        Task<long> Upload(EnumObjectType objectTypeId, string fileName, IFormFile file);

        Task<bool> DeleteFile(long fileId);
        Task<bool> FileAssignToObject(EnumObjectType objectTypeId, long objectId, long fileId);

        Task<FileToDownloadInfo> GetFileUrl(long fileId, EnumThumbnailSize? thumb);

        Task<(FileEnity info, string physicalPath)> GetFileAndPath(long fileId);

        Task<IList<FileThumbnailInfo>> GetThumbnails(IList<long> fileIds, EnumThumbnailSize? thumb);

        Task<bool> GenerateThumbnail(long fileId);
        

        Task<IList<FileToDownloadInfo>> GetListFileUrl(IList<long> fileIds, EnumThumbnailSize? thumb);

        IList<ExcelSheetDataModel> ParseExcel(IFormFile file, string sheetName, int fromRow = 1, int? toRow = null, int? maxrows = null);
    }
}
