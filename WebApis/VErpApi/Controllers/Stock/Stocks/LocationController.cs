using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Location;
using VErp.Services.Stock.Service.Location;

namespace VErpApi.Controllers.Stock.Stocks
{
    [Route("api/locations")]

    public class LocationController : VErpBaseController
    {
        private readonly ILocationService _locationService;
        public LocationController(ILocationService locationService
            )
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Tìm kiếm vị trí theo kho 
        /// </summary>
        /// <param name="stockId">Id kho</param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<LocationOutput>>> Get([FromQuery] int stockId,[FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _locationService.GetList(stockId,keyword, page, size);
        }


        /// <summary>
        /// Thêm mới vị trí vào kho 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddLocation([FromBody] LocationInput location)
        {
            return await _locationService.AddLocation(location);
        }

        /// <summary>
        /// Lấy thông tin kho sản phẩm
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{locationId}")]
        public async Task<ApiResponse<LocationOutput>> GetLocation([FromRoute] int locationId)
        {
            return await _locationService.GetLocationInfo(locationId);
        }

        /// <summary>
        /// Cập nhật thông tin vị trí trong kho
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{locationId}")]
        public async Task<ApiResponse> UpdateLocation([FromRoute] int locationId, [FromBody] LocationInput location)
        {
            return await _locationService.UpdateLocation(locationId, location);
        }

        /// <summary>
        /// Xóa thông tin vị trí trong kho
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{locationId}")]
        public async Task<ApiResponse> Delete([FromRoute] int locationId)
        {
            return await _locationService.DeleteLocation(locationId);
        }
    }
}