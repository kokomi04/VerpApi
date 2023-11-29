using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig
{
    public interface IPrintConfigService<TModel>
    {
        Task<PageData<TModel>> Search(int? moduleTypeId, IList<int> printConfigIds, string keyword, int page, int size, string orderByField, bool asc);

        Task<TModel> GetPrintConfig(int printConfigId);
        Task<TModel> GetPrintConfigByName(string name);

        Task<int> AddPrintConfig(TModel model, IFormFile template, IFormFile background);
        Task<bool> UpdatePrintConfig(int printConfigId, TModel model, IFormFile template, IFormFile background);
        Task<bool> DeletePrintConfig(int printConfigId);

        Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId);
        Task<(Stream file, string contentType, string fileName)> GetPrintConfigBackgroundFile(int printConfigId);

        Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, NonCamelCaseDictionary templateModel, bool isDoc);

    }
}
