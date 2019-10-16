using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileService
    {
        Task<ServiceResult<long>> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file);
        Task<Enum> FileAssignToObject(EnumObjectType objectTypeId, long objectId, long fileId);
    }
}
