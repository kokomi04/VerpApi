using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IBarcodeConfigService
    {
        Task<ServiceResult<int>> AddBarcodeConfig(BarcodeConfigModel data);
        Task<PageData<BarcodeConfigListOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<BarcodeConfigModel>> GetInfo(int barcodeConfigId);
        Task<Enum> UpdateBarcodeConfig(int barcodeConfigId, BarcodeConfigModel data);
        Task<Enum> DeleteBarcodeConfig(int barcodeConfigId);
        Task<ServiceResult<string>> Make(int barcodeConfigId, int productCode);
    }
}
