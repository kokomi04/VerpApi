using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionStepLinkData
{
    public long ProductionStepLinkDataId { get; set; }

    public string ProductionStepLinkDataCode { get; set; }

    /// <summary>
    /// 1-GC chi tiet, 2-GC cong doan, 0-default
    /// </summary>
    public int ProductionStepLinkDataTypeId { get; set; }

    public long? ObjectIdBak { get; set; }

    public int? ObjectTypeIdBak { get; set; }

    /// <summary>
    /// Số lượng sản xuất
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Số lượng gốc (trong thiết kế QTSX ban đầu khi chưa kéo)
    /// </summary>
    public decimal QuantityOrigin { get; set; }

    /// <summary>
    /// Số lượng gia công công đoạn
    /// </summary>
    public decimal? OutsourceQuantity { get; set; }

    /// <summary>
    /// Số lượng NVL đầu vào của nhóm công đoạn đi gia công công đoạn cần xuất đi gia công
    /// </summary>
    public decimal? ExportOutsourceQuantity { get; set; }

    /// <summary>
    /// Số lượng gia công chi tiết
    /// </summary>
    public decimal? OutsourcePartQuantity { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public long? OutsourceRequestDetailId { get; set; }

    public int ProductionStepLinkTypeId { get; set; }

    public long? ConverterId { get; set; }

    public decimal? WorkloadConvertRate { get; set; }

    public long LinkDataObjectId { get; set; }

    public int LinkDataObjectTypeId { get; set; }

    public long? ProductionOutsourcePartMappingId { get; set; }

    public virtual OutsourcePartRequestDetail OutsourceRequestDetail { get; set; }

    public virtual ICollection<OutsourceStepRequestData> OutsourceStepRequestData { get; set; } = new List<OutsourceStepRequestData>();

    public virtual ICollection<ProductionAssignment> ProductionAssignment { get; set; } = new List<ProductionAssignment>();

    public virtual ICollection<ProductionOrderMaterials> ProductionOrderMaterials { get; set; } = new List<ProductionOrderMaterials>();

    public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; } = new List<ProductionStepLinkDataRole>();
}
