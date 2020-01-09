using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Service.Dictionary;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/units")]

    public class UnitsController : VErpBaseController
    {
        private readonly IUnitService _unitService;
        public UnitsController(IUnitService unitService
            )
        {
            _unitService = unitService;
        }

        /// <summary>
        /// Lấy danh sách đơn vị tính
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<UnitOutput>>> Get([FromQuery] string keyword, [FromQuery] EnumUnitStatus? unitStatusId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _unitService.GetList(keyword, unitStatusId, page, size);
        }

        /// <summary>
        /// Thêm mới đơn vị tính
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddUnit([FromBody] UnitInput unit)
        {
            return await _unitService.AddUnit(unit);
        }

        /// <summary>
        /// Lấy thông tin đơn vị tính
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{unitId}")]
        public async Task<ApiResponse<UnitOutput>> GetUnitInfo([FromRoute] int unitId)
        {
            return await _unitService.GetUnitInfo(unitId);
        }

        /// <summary>
        /// Cập nhật thông tin đơn vị tính
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{unitId}")]
        public async Task<ApiResponse> UpdateUnit([FromRoute] int unitId, [FromBody] UnitInput unit)
        {
            return await _unitService.UpdateUnit(unitId, unit);
        }

        /// <summary>
        /// Xóa đơn vị tính
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{unitId}")]
        public async Task<ApiResponse> DeleteUnit([FromRoute] int unitId)
        {
            return await _unitService.DeleteUnit(unitId);
        }
    }
}