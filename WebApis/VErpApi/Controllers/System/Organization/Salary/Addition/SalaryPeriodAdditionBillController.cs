using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.System.Organization.Salary.Addition
{
    [Route("api/organization/salary/addition/data")]
    public class SalaryPeriodAdditionBillController : VErpBaseController
    {
        private readonly ISalaryPeriodAdditionBillService _salaryPeriodAdditionBillService;
        private readonly ISalaryPeriodAdditionBillImportService _salaryPeriodAdditionBillImportService;
        private readonly ISalaryPeriodAdditionBillExportService _salaryPeriodAdditionBillExportService;
        public SalaryPeriodAdditionBillController(ISalaryPeriodAdditionBillService salaryPeriodAdditionBillService, ISalaryPeriodAdditionBillImportService salaryPeriodAdditionBillImportService, ISalaryPeriodAdditionBillExportService salaryPeriodAdditionBillExportService)
        {
            _salaryPeriodAdditionBillService = salaryPeriodAdditionBillService;
            _salaryPeriodAdditionBillImportService = salaryPeriodAdditionBillImportService;
            _salaryPeriodAdditionBillExportService = salaryPeriodAdditionBillExportService;
        }


        [HttpGet("{salaryPeriodAdditionTypeId}/bills")]
        public async Task<PageData<SalaryPeriodAdditionBillList>> GetList([FromRoute] int salaryPeriodAdditionTypeId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _salaryPeriodAdditionBillService.GetList(salaryPeriodAdditionTypeId, year, month, keyword, page, size);
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


        [HttpGet("{salaryPeriodAdditionTypeId}/bills/export")]
        public async Task<(Stream stream, string fileName, string contentType)> Export([FromRoute] int salaryPeriodAdditionTypeId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _salaryPeriodAdditionBillExportService.Export(salaryPeriodAdditionTypeId, year, month, keyword);
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
    }
}