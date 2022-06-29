using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.package
{
    [Route("api/stock/PackageCustomProperty")]

    public class PackageCustomPropertyController : VErpBaseController
    {
        private readonly IPackageCustomPropertyService _packageCustomPropertyService;

        public PackageCustomPropertyController(IPackageCustomPropertyService packageCustomPropertyService)
        {
            _packageCustomPropertyService = packageCustomPropertyService;
        }

        [HttpGet]
        public Task<IList<PackageCustomPropertyModel>> Get()
        {
            return _packageCustomPropertyService.Get();
        }

        [HttpGet("{packageCustomPropertyId}")]
        public Task<PackageCustomPropertyModel> Info([FromRoute] int packageCustomPropertyId)
        {
            return _packageCustomPropertyService.Info(packageCustomPropertyId);

        }

        [HttpPost]
        public Task<int> Create([FromBody] PackageCustomPropertyModel model)
        {
            return _packageCustomPropertyService.Create(model);
        }

        [HttpPut("{packageCustomPropertyId}")]
        public Task<bool> Update([FromRoute] int packageCustomPropertyId, [FromBody] PackageCustomPropertyModel model)
        {
            return _packageCustomPropertyService.Update(packageCustomPropertyId, model);
        }

        [HttpDelete("{packageCustomPropertyId}")]
        public Task<bool> Delete([FromRoute] int packageCustomPropertyId)
        {
            return _packageCustomPropertyService.Delete(packageCustomPropertyId);
        }

    }
}