using System;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Model.Stock;

namespace VErp.Services.Stock.Model.Inventory.InventoryRequirement
{
    public class InventoryRequirementFileInputModel : IMapFrom<InventoryRequirementFile>
    {
        public long InventoryRequirementId { get; set; }
        public long FileId { get; set; }
        public InventoryRequirementFileInputModel()
        {
        }
    }

    public class InventoryRequirementFileOutputModel : InventoryRequirementFileInputModel
    {
        public FileToDownloadInfo FileToDownloadInfo { get; set; }
        public InventoryRequirementFileOutputModel()
        {
        }
    }
}
