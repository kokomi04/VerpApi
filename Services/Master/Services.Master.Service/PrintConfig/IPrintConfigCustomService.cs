using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig
{
    public interface IPrintConfigCustomService
    {
        Task<PageData<PrintConfigCustomModel>> Search(int moduleTypeId, string keyword, int page, int size, string orderByField, bool asc);

        Task<PrintConfigCustomModel> GetPrintConfigCustom(int printConfigId);
        Task<int> AddPrintConfigCustom(PrintConfigCustomModel model, IFormFile file);
        Task<bool> UpdatePrintConfigCustom(int printConfigId, PrintConfigCustomModel model, IFormFile file);
        Task<bool> DeletePrintConfigCustom(int printConfigId);

        Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId);
        Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, NonCamelCaseDictionary templateModel, bool isDoc);

        Task<bool> RollbackPrintConfigCustom(int printConfigId);
    }
}
