using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.OutsideMapping;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IOutsideImportMappingService
    {
        Task<PageData<OutsideMappingModelList>> GetList(string keyword, int page, int size);

        Task<int> CreateImportMapping(OutsideMappingModel model);

        Task<OutsideMappingModel> GetImportMappingInfo(int outsideImportMappingFunctionId);

        Task<OutsideMappingModel> GetImportMappingInfo(string mappingFunctionKey);

        Task<bool> UpdateImportMapping(int outsideImportMappingFunctionId, OutsideMappingModel model);

        Task<bool> DeleteImportMapping(int outsideImportMappingFunctionId);

        Task<OutsideImportMappingObjectModel> MappingObjectInfo(string mappingFunctionKey, string objectId);

        Task<bool> MappingObjectCreate(string mappingFunctionKey, string objectId, long inputBillFId);

        Task<bool> MappingObjectDelete(long inputBillFId);
    }
}
