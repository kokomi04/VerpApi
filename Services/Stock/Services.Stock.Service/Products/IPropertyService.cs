using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products
{
    public interface IPropertyService
    {
        Task<IList<PropertyModel>> GetProperties();
        Task<PropertyModel> GetProperty(int propertyId);
        Task<int> CreateProperty(PropertyModel req);
        Task<int> UpdateProperty(int propertyId, PropertyModel req);
        Task<bool> DeleteProperty(int propertyId);
    }
}
