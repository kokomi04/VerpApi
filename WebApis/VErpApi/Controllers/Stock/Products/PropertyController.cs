using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/property")]
    public class PropertyController : VErpBaseController
    {
        private readonly IPropertyService _propertyService;
        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<IList<PropertyModel>> GetProperties()
        {
            return await _propertyService.GetProperties();
        }

        [HttpGet]
        [Route("{propertyId}")]
        public async Task<PropertyModel> GetProperty([FromRoute] int propertyId)
        {
            return await _propertyService.GetProperty(propertyId);
        }


        [HttpPost]
        [Route("")]
        public async Task<int> CreateProperty([FromBody] PropertyModel model)
        {
            return await _propertyService.CreateProperty(model);
        }

        [HttpPut]
        [Route("{propertyId}")]
        public async Task<int> UpdateProperty([FromRoute] int propertyId, [FromBody] PropertyModel model)
        {
            return await _propertyService.UpdateProperty(propertyId, model);
        }

        [HttpDelete]
        [Route("{propertyId}")]
        public async Task<bool> DeleteProperty([FromRoute] int propertyId)
        {
            return await _propertyService.DeleteProperty(propertyId);
        }
    }
}
