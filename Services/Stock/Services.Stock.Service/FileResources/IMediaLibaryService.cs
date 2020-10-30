using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IMediaLibaryService
    {
        Task<PageData<VisualFile>> GetVisualFiles(string directory, string keyWord, int page, int size);
        Task<DirectoryStructure> GetDirectoryStructure();
        Task<bool> CreateSubdirectory(string root, string subdirectory);



        Task<bool> UploadFiles(string directory, IEnumerable<IFormFile> formFiles);
        Task<bool> UploadFile(string directory, IFormFile file);
        Task<bool> DeleteFiles(IList<VisualFile> files);

        Task<(Stream file, string fileName, string contentType)> GetFileStream(string filePath, bool thumb);
    }
}
