using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;
using FileEnity = VErp.Infrastructure.EF.StockDB.File;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileService
    {
        Task<ServiceResult<long>> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file);
        Task<ServiceResult<long>> Upload(EnumObjectType objectTypeId, string fileName, IFormFile file);

        Task<Enum> DeleteFile(long fileId);
        Task<Enum> FileAssignToObject(EnumObjectType objectTypeId, long objectId, long fileId);

        Task<ServiceResult<FileToDownloadInfo>> GetFileUrl(long fileId, EnumThumbnailSize? thumb);

        Task<ServiceResult<(FileEnity info, string physicalPath)>> GetFileAndPath(long fileId);

        Task<ServiceResult<IList<FileThumbnailInfo>>> GetThumbnails(IList<long> fileIds, EnumThumbnailSize? thumb);

        Task<Enum> GenerateThumbnail(long fileId);
        

        List<FileToDownloadInfo> GetListFileUrl(long[] arrayFileId, EnumThumbnailSize? thumb);
    }
}
