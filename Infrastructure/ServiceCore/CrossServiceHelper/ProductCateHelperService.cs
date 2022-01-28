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
