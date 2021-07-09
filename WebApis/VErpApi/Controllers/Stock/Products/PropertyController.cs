using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
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
