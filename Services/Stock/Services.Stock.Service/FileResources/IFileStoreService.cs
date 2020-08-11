using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileStoreService
    {
        Task<(Stream file, string contentType)> GetFileStream(string fileKey);
    }
}
