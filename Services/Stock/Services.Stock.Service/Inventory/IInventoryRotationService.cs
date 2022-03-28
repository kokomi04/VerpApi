using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Inventory
{
    public interface IInventoryRotationService
    {
        Task<long> Create(InventoryOutRotationModel req);

        Task<bool> SentToCensor(long inventoryId);

        Task<bool> Reject(long inventoryId);

        Task<bool> Approve(long inventoryId);

        Task<bool> NotApprovedDelete(long outInvId);

        Task<bool> ApprovedDelete(long outInvId, long fromDate, long toDate, ApprovedInputDataSubmitModel req);

        
    }
}
