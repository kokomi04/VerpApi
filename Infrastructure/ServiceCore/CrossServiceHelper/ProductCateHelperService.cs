using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductCateHelperService
    {
        Task<PageData<InternalProductCateOutput>> Search(Clause filters, string keyword, int page, int size, string orderBy, bool asc);
    }
    public class ProductCateHelperService : IProductCateHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductCateHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<PageData<InternalProductCateOutput>> Search(Clause filters, string keyword, int page, int size, string orderBy, bool asc)
        {
            return await _httpCrossService.Post<PageData<InternalProductCateOutput>>($"api/internal/InternalProductCate?keyword={keyword}&page={page}&size={size}&orderBy={orderBy}&asc={asc}", filters);
        }
    }
}
