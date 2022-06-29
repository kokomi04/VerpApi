using System.IO;
using System.Threading.Tasks;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileStoreService
    {
        Task<(Stream file, string contentType)> GetFileStream(string fileKey);
    }
}
