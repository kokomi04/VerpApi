using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;
using System.Data;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductBomHelperService
    {
        Task<IList<InternalProductElementModel>> GetElements(int[] productIds);
    }

    public class ProductBomHelperService : IProductBomHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductBomHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<IList<InternalProductElementModel>> GetElements(int[] productIds)
        {
            return await _httpCrossService.Post<IList<InternalProductElementModel>>($"api/internal/InternalProductBom/products", productIds);
        }
    }
}
