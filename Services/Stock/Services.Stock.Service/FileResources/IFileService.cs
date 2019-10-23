using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;
using FileEnity = VErp.Infrastructure.EF.StockDB.File;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileService
    {
        Task<ServiceResult<long>> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file);
        Task<Enum> FileAssignToObject(EnumObjectType objectTypeId, long objectId, long fileId);
        Task<ServiceResult<(FileEnity info, Stream file)>> GetFileStream(long fileId);

        Task<ServiceResult<FileToDownloadInfo>> GetFileUrl(long fileId);

        Task<ServiceResult<(Stream file, string contentType)>> GetFileStream(string fileKey);
    }
}
