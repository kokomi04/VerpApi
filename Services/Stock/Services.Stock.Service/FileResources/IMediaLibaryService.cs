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
        Task<bool> DeletedDirectory(string d);
        Task<bool> CopyDirectory(string d, string n);
        Task<bool> RenameDirectory(string d, string n);

        Task<bool> UploadFiles(string directory, IEnumerable<IFormFile> formFiles);
        Task<bool> DeleteFiles(IList<string> files);
        Task<bool> CopyFiles(IList<string> files, string directory);
        Task<bool> MoveFiles(IList<string> files, string directory);
        Task<bool> RenameFile(string f, string n);

        Task<(Stream file, string fileName, string contentType)> GetFileStream(string filePath, bool thumb);
        Task<bool> MoveDirectory(string directory, string newDirectory);
    }
}
