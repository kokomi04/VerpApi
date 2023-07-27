using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionHandover
{
    public long ProductionHandoverId { get; set; }

    public long? ProductionHandoverReceiptId { get; set; }

    public int SubsidiaryId { get; set; }

    public DateTime? HandoverDatetime { get; set; }

    public long ProductionOrderId { get; set; }

    public long? ProductionStepLinkDataId { get; set; }

    public long ObjectId { get; set; }

    public int ObjectTypeId { get; set; }

    public int FromDepartmentId { get; set; }

    public long FromProductionStepId { get; set; }

    public int ToDepartmentId { get; set; }

    public long ToProductionStepId { get; set; }

    public decimal HandoverQuantity { get; set; }

    public string Note { get; set; }

    public int Status { get; set; }

    public long? InventoryRequirementDetailId { get; set; }

    public long? InventoryDetailId { get; set; }

    /// <summary>
    /// Product id in production process
    /// </summary>
    public int? InventoryProductId { get; set; }

    public bool IsAuto { get; set; }

    public int? AcceptByUserId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int RowIndex { get; set; }

    public long? InventoryId { get; set; }

    public string InventoryCode { get; set; }

    public decimal? InventoryQuantity { get; set; }

    public virtual ProductionStep FromProductionStep { get; set; }

    public virtual ProductionHandoverReceipt ProductionHandoverReceipt { get; set; }

    public virtual ProductionOrder ProductionOrder { get; set; }

    public virtual ProductionStep ToProductionStep { get; set; }
}
