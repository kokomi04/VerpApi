using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary.Addition
{
    [Route("api/organization/salary/addition/data")]
    public class SalaryPeriodAdditionBillController : VErpBaseController
    {
        private readonly ISalaryPeriodAdditionBillService _salaryPeriodAdditionBillService;
        private readonly ISalaryPeriodAdditionBillImportService _salaryPeriodAdditionBillImportService;
        private readonly ISalaryPeriodAdditionBillParseService _salaryPeriodAdditionBillParseService;
        private readonly ISalaryPeriodAdditionBillExportService _salaryPeriodAdditionBillExportService;
        public SalaryPeriodAdditionBillController(ISalaryPeriodAdditionBillService salaryPeriodAdditionBillService, ISalaryPeriodAdditionBillImportService salaryPeriodAdditionBillImportService, ISalaryPeriodAdditionBillExportService salaryPeriodAdditionBillExportService, ISalaryPeriodAdditionBillParseService salaryPeriodAdditionBillParseService)
        {
            _salaryPeriodAdditionBillService = salaryPeriodAdditionBillService;
            _salaryPeriodAdditionBillImportService = salaryPeriodAdditionBillImportService;
            _salaryPeriodAdditionBillExportService = salaryPeriodAdditionBillExportService;
            _salaryPeriodAdditionBillParseService = salaryPeriodAdditionBillParseService;
        }


        [HttpGet("{salaryPeriodAdditionTypeId}/bills")]
        public async Task<PageData<SalaryPeriodAdditionBillList>> GetList([FromRoute] int salaryPeriodAdditionTypeId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            var req = new SalaryPeriodAdditionBillsRequestModel()
            {
                Year = year,
                Month = month,
                Size = size,
                Page = page,
                Keyword = keyword
            };

            return await _salaryPeriodAdditionBillService.GetList(salaryPeriodAdditionTypeId, req);
        }


        [HttpPost("{salaryPeriodAdditionTypeId}/bills/Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<SalaryPeriodAdditionBillList>> GetList([FromRoute] int salaryPeriodAdditionTypeId, [FromBody] SalaryPeriodAdditionBillsRequestModel req)
        {
            return await _salaryPeriodAdditionBillService.GetList(salaryPeriodAdditionTypeId, req);
        }

        [HttpGet("{salaryPeriodAdditionTypeId}/bills/export")]
        public async Task<IActionResult> Export([FromRoute] int salaryPeriodAdditionTypeId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] string keyword)
        {
            var req = new SalaryPeriodAdditionBillsExportModel()
            {
                Year = year,
                Month = month,
                Keyword = keyword
            };

            var (stream, fileName, contentType) = await _salaryPeriodAdditionBillExportService.Export(salaryPeriodAdditionTypeId, req);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpPost("{salaryPeriodAdditionTypeId}/bills/export")]
        [VErpAction(EnumActionType.View)]
        public async Task<IActionResult> Export([FromRoute] int salaryPeriodAdditionTypeId, [FromBody] SalaryPeriodAdditionBillsExportModel req)
        {
            var (stream, fileName, contentType) = await _salaryPeriodAdditionBillExportService.Export(salaryPeriodAdditionTypeId, req);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }


        [HttpGet("{salaryPeriodAdditionTypeId}/bills/{salaryPeriodAdditionBillId}")]
        public async Task<SalaryPeriodAdditionBillInfo> GetList([FromRoute] int salaryPeriodAdditionTypeId, [FromRoute] long salaryPeriodAdditionBillId)
        {
            return await _salaryPeriodAdditionBillService.GetInfo(salaryPeriodAdditionTypeId, salaryPeriodAdditionBillId);
        }


        [HttpPost("{salaryPeriodAdditionTypeId}/bills")]
        public async Task<long> Create([FromRoute] int salaryPeriodAdditionTypeId, [FromBody] SalaryPeriodAdditionBillModel model)
        {
            return await _salaryPeriodAdditionBillService.Create(salaryPeriodAdditionTypeId, model);
        }

        [HttpPut("{salaryPeriodAdditionTypeId}/bills/{salaryPeriodAdditionBillId}")]
        public async Task<bool> Update([FromRoute] int salaryPeriodAdditionTypeId, [FromRoute] long salaryPeriodAdditionBillId, [FromBody] SalaryPeriodAdditionBillModel model)
        {
            return await _salaryPeriodAdditionBillService.Update(salaryPeriodAdditionTypeId, salaryPeriodAdditionBillId, model);
        }

        [HttpDelete("{salaryPeriodAdditionTypeId}/bills/{salaryPeriodAdditionBillId}")]
        public async Task<bool> Delete([FromRoute] int salaryPeriodAdditionTypeId, [FromRoute] long salaryPeriodAdditionBillId)
        {
            return await _salaryPeriodAdditionBillService.Delete(salaryPeriodAdditionTypeId, salaryPeriodAdditionBillId);
        }




        [HttpGet]
        [Route("{salaryPeriodAdditionTypeId}/bills/fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping([FromRoute] int salaryPeriodAdditionTypeId)
        {
            return await _salaryPeriodAdditionBillImportService.GetFieldDataForMapping(salaryPeriodAdditionTypeId);
        }

        [HttpPost]
        [Route("{salaryPeriodAdditionTypeId}/bills/importFromMapping")]
        public async Task<bool> ImportFromMapping([FromRoute] int salaryPeriodAdditionTypeId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _salaryPeriodAdditionBillImportService.Import(salaryPeriodAdditionTypeId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{salaryPeriodAdditionTypeId}/bills/fieldForParse")]
        public async Task<CategoryNameModel> GetFieldDataMappingForParse([FromRoute] int salaryPeriodAdditionTypeId)
        {
            return await _salaryPeriodAdditionBillParseService.GetFieldDataMappingForParse(salaryPeriodAdditionTypeId);
        }

        [HttpPost]
        [Route("{salaryPeriodAdditionTypeId}/bills/parseExcelFromMapping")]
        public async Task<IList<SalaryPeriodAdditionBillEmployeeParseInfo>> parseExcelFromMapping([FromRoute] int salaryPeriodAdditionTypeId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _salaryPeriodAdditionBillParseService.ParseExcel(salaryPeriodAdditionTypeId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }
    }
}