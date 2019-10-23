using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Stock.Files
{
    [Route("api/files/images")]
    public class ImagesController : VErpBaseController
    {
        private readonly IFileService _fileService;
        public ImagesController(IFileService fileService
            )
        {
            _fileService = fileService;
        }

        [GlobalApi]
        [Route("view")]
        public async Task<IActionResult> View([FromQuery] long fileId)
        {
            var r = await _fileService.GetFileStream(fileId);
            if (!r.Code.IsSuccess())
            {
                return new ObjectResult(r);
            }

            return new FileStreamResult(r.Data.file, r.Data.info.ContentType);
        }
    }
}