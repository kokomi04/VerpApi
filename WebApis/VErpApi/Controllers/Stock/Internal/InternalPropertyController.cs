using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalPropertyController : CrossServiceBaseController
    {
        private readonly IPropertyService _propertyService;
        public InternalPropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }



        [HttpGet]
        [Route("{propertyId}")]
        public async Task<PropertyInfoModel> GetProduct([FromRoute] int propertyId)
        {
            return await _propertyService.GetProperty(propertyId);
        }

        [HttpPost]
        [Route("GetByIds")]
        public async Task<IList<PropertyInfoModel>> GetByIds([FromBody] IList<int> propertyIds)
        {
            return (await _propertyService.GetByIds(propertyIds))?.Select(s => (PropertyInfoModel)s)?.ToList();
        }

    }
}