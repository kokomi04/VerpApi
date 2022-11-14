using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Services.Stock.Model.FileResources;
using FileEnity = VErp.Infrastructure.EF.StockDB.File;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface ITemplateFileService
    {
        IList<TemplateFileToDownloadInfo> GetFilesUrls(IList<string> filePaths);

        Task<string> Upload(EnumFileType fileTypeId, IFormFile file);
    }
}
