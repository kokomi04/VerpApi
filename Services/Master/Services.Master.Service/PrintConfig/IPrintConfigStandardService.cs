﻿using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig
{
    public interface IPrintConfigStandardService
    {
        Task<PageData<PrintConfigStandardModel>> Search(int moduleTypeId, string keyword, int page, int size, string orderByField, bool asc);

        Task<PrintConfigStandardModel> GetPrintConfigStandard(int printConfigId);
        Task<int> AddPrintConfigStandard(PrintConfigStandardModel model, IFormFile file);
        Task<bool> UpdatePrintConfigStandard(int printConfigId, PrintConfigStandardModel model, IFormFile file);
        Task<bool> DeletePrintConfigStandard(int printConfigId);

        Task<(Stream file, string contentType, string fileName)> GetPrintConfigTemplateFile(int printConfigId);
        Task<(Stream file, string contentType, string fileName)> GeneratePrintTemplate(int printConfigId, NonCamelCaseDictionary templateModel, bool isDoc);
    }
}
