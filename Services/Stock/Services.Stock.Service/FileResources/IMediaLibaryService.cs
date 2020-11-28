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
        PageData<VisualFile> GetVisualFiles(string directory, string keyWord, int page, int size);
        DirectoryStructure GetDirectoryStructure();
        bool CreateSubdirectory(string root, string subdirectory);
        bool DeletedDirectory(string directory);
        bool CopyDirectory(string pathSource, string pathDest);
        bool RenameDirectory(string directory, string newNameFolder);

        Task<bool> UploadFiles(string directory, IEnumerable<IFormFile> formFiles);
        bool DeleteFiles(IList<string> files);
        bool CopyFiles(IList<string> files, string directory);
        bool MoveFiles(IList<string> files, string directory);
        bool RenameFile(string filePath, string newNameFile);

        (Stream file, string fileName, string contentType) GetFileStream(string filePath, bool thumb);
        bool MoveDirectory(string directory, string newDirectory);
    }
}
