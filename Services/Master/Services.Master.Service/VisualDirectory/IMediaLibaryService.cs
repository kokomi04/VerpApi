using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.VisualDirectory;

namespace VErp.Services.Master.Service.VisualDirectory
{
    public interface IMediaLibaryService
    {
        Task<PageData<VisualFile>> GetVisualFiles(string directory, string keyWord, int page, int size);
        Task<DirectoryStructure> GetDirectoryStructure();
        Task<bool> UploadFiles(string directory, IEnumerable<IFormFile> formFiles);
        Task<bool> UploadFile(string directory, IFormFile file);
        Task<bool> DeleteFiles(IList<VisualFile> files);
        Task<bool> CreateSubdirectory(string root, string subdirectory);
    }
}
