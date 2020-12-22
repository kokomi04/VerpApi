using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IPrintConfigService
    {
        Task<PrintConfigModel> GetPrintConfig(int printConfigId);
        Task<ICollection<PrintConfigModel>> GetPrintConfigs(int moduleTypeId ,int activeForId);
        Task<int> AddPrintConfig(PrintConfigModel data);
        Task<bool> UpdatePrintConfig(int printConfigId, PrintConfigModel data);
        Task<bool> DeletePrintConfig(int printConfigId);
        Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, int fileId, PrintTemplateInput templateModel);
        Task<IList<EntityField>> GetSuggestionField(int moduleTypeId);
    }
}
