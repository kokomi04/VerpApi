using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalFileController : CrossServiceBaseController
    {
        private readonly IFileService _fileService;
        public InternalFileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [Route("{fileId}/FileAssignToObject")]
        [HttpPut]
        public async Task<bool> FileAssignToObject([FromRoute] long fileId, [FromBody] FileAssignToObjectInput req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            return await _fileService.FileAssignToObject(req.ObjectTypeId, req.ObjectId, fileId);
        }

        [Route("{fileId}")]
        [HttpDelete]
        public async Task<bool> DeleteFile([FromRoute] long fileId)
        {
            return await _fileService.DeleteFile(fileId);
        }

        [HttpPost]
        [Route("{objectTypeId}")]
        public async Task<long> SaveSimpleFileInfo([FromRoute] EnumObjectType objectTypeId, [FromBody] SimpleFileInfo simpleFile)
        {
            return await _fileService.SaveFileInfo(objectTypeId, simpleFile);
        }

        [HttpGet]
        [Route("{fileId}")]
        public async Task<SimpleFileInfo> GetSimpleFileInfo([FromRoute] long fileId)
        {
            return await _fileService.GetSimpleFileInfo(fileId);
        }
    }
}