using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
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
        public async Task<ServiceResult> FileAssignToObject([FromRoute] long fileId, [FromBody] FileAssignToObjectInput req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            return await _fileService.FileAssignToObject(req.ObjectTypeId, req.ObjectId, fileId);
        }

        [Route("{fileId}")]
        [HttpDelete]
        public async Task<ServiceResult> DeleteFile([FromRoute] long fileId)
        {
            return await _fileService.DeleteFile(fileId);
        }
    }
}