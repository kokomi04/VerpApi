using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IBarcodeConfigService
    {
        Task<int> AddBarcodeConfig(BarcodeConfigModel data);
        Task<PageData<BarcodeConfigListOutput>> GetList(string keyword, int page, int size);
        Task<IList<BarcodeConfigListOutput>> GetListActived();
        Task<BarcodeConfigModel> GetInfo(int barcodeConfigId);
        Task<bool> UpdateBarcodeConfig(int barcodeConfigId, BarcodeConfigModel data);
        Task<bool> DeleteBarcodeConfig(int barcodeConfigId);
        Task<string> Make(int barcodeConfigId);
    }
}
