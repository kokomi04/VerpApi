using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductBomHelperService
    {
        Task<IList<InternalProductElementModel>> GetElements(int[] productIds);
        Task<IList<ProductBomBaseSimple>> GetBOM(int productId);
        Task<IDictionary<int, IList<ProductBomBaseSimple>>> GetBOMs(int[] productId);

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

        public async Task<IList<ProductBomBaseSimple>> GetBOM(int productId)
        {
            return await _httpCrossService.Get<IList<ProductBomBaseSimple>>($"api/internal/InternalProductBom/{productId}");
        }

        public async Task<IDictionary<int, IList<ProductBomBaseSimple>>> GetBOMs(int[] productId)
        {
            return await _httpCrossService.Post<IDictionary<int, IList<ProductBomBaseSimple>>>($"api/internal/InternalProductBom/ByProductIds", productId);
        }
    }
}
