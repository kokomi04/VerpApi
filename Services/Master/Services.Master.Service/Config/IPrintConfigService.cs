using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IPrintConfigService
    {
        Task<PrintConfigModel> GetPrintConfig(int printConfigId, bool isOrigin);
        Task<ICollection<PrintConfigModel>> GetPrintConfigs(int moduleTypeId);
        Task<int> AddPrintConfig(PrintConfigModel data);
        Task<bool> UpdatePrintConfig(int printConfigId, PrintConfigModel data);
        Task<bool> DeletePrintConfig(int printConfigId);
        Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, int fileId, PrintTemplateInput templateModel);
        Task<IList<EntityField>> GetSuggestionField(int moduleTypeId);
        Task<IList<EntityField>> GetSuggestionField(Assembly assembly);
        Task<long> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file);
        Task<bool> RollbackPrintConfig(long printConfigId);
    }
}
