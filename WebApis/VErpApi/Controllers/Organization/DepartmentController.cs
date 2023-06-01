using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Department;
using VErp.Services.Organization.Service.Department;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Organization
{
    [Route("api/departments")]
    public class DepartmentController : VErpBaseController
    {
        private readonly IDepartmentService _departmentService;
        private readonly IFileService _fileService;

        public DepartmentController(IDepartmentService departmentService, IFileService fileService)
        {
            _departmentService = departmentService;
            _fileService = fileService;
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<PageData<DepartmentModel>> Get([FromQuery] string keyword, [FromQuery] IList<int> departmentIds, [FromQuery] bool? isProduction, [FromQuery] bool? isActived, [FromQuery] int page, [FromQuery] int size)
        {
            return await _departmentService.GetList(keyword, departmentIds, isProduction, isActived, page, size);
        }


        [HttpPost]
        [Route("GetInfosByIds")]
        [GlobalApi]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<DepartmentModel>> GetInfosByIds([FromBody] IList<int> departmentIds)
        {
            return await _departmentService.GetListByIds(departmentIds);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddDepartment([FromBody] DepartmentModel department)
        {
            return await _departmentService.AddDepartment(department);
        }

        [HttpGet]
        [Route("{departmentId}")]
        public async Task<DepartmentModel> GetDepartmentInfo([FromRoute] int departmentId)
        {
            return await _departmentService.GetDepartmentInfo(departmentId);
        }

        [HttpPut]
        [Route("{departmentId}")]
        public async Task<bool> UpdateDepartment([FromRoute] int departmentId, [FromBody] DepartmentModel department)
        {
            return await _departmentService.UpdateDepartment(departmentId, department);
        }

        [HttpDelete]
        [Route("{departmentId}")]
        public async Task<bool> DeleteDepartment([FromRoute] int departmentId)
        {
            return await _departmentService.DeleteDepartment(departmentId);
        }

        /// <summary>
        /// Upload department image
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("image")]
        public async Task<long> Image([FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.Department, EnumFileType.Image, string.Empty, file).ConfigureAwait(true);
        }

    }
}