﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Stock.Files
{
    [Route("api/files")]
    public class FilesController : VErpBaseController
    {
        private readonly IFileService _fileService;
        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [GlobalApi]
        [HttpGet]
        [Route("{fileId}/GetFileUrl")]
        public async Task<ApiResponse<string>> GetFileUrl([FromRoute] long fileId)
        {
            return await _fileService.GetFileUrl(fileId);
        }

        [AllowAnonymous]
        [Route("Preview")]
        [HttpGet]
        public async Task<IActionResult> Preview([FromQuery] string fileKey)
        {
            var r = await _fileService.GetFileStream(fileKey);
            if (!r.Code.IsSuccess())
            {
                return new JsonResult(r);
            }

            return new FileStreamResult(r.Data.file, !string.IsNullOrWhiteSpace(r.Data.contentType) ? r.Data.contentType : "application/octet-stream");
        }
    }
}