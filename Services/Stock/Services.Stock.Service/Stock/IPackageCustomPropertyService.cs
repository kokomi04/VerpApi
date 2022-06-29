using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IPackageCustomPropertyService
    {
        Task<IList<PackageCustomPropertyModel>> Get();
        Task<int> Create(PackageCustomPropertyModel model);
        Task<bool> Update(int packageCustomPropertyId, PackageCustomPropertyModel model);
        Task<bool> Delete(int packageCustomPropertyId);
        Task<PackageCustomPropertyModel> Info(int packageCustomPropertyId);
    }
}
