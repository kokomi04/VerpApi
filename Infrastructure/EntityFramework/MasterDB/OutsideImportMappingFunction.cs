using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class OutsideImportMappingFunction
{
    public int OutsideImportMappingFunctionId { get; set; }

    public int? SourceObjectTypeId { get; set; }

    public int? SourceInputTypeId { get; set; }

    public int ObjectTypeId { get; set; }

    public int InputTypeId { get; set; }

    public string MappingFunctionKey { get; set; }

    public string FunctionName { get; set; }

    public string Description { get; set; }

    public bool IsWarningOnDuplicated { get; set; }

    public string SourceDetailsPropertyName { get; set; }

    public string DestinationDetailsPropertyName { get; set; }

    public string ObjectIdFieldName { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public string JsCodeAfterSourceDataLoaded { get; set; }

    public string JsCodeBeforeDataMapped { get; set; }

    public string JsCodeAfterDataMapped { get; set; }

    public string JsCodeAfterTargetBillCreated { get; set; }

    public string JsCodeVisible { get; set; }

    public virtual ICollection<OutsideImportMapping> OutsideImportMapping { get; set; } = new List<OutsideImportMapping>();

    public virtual ICollection<OutsideImportMappingObject> OutsideImportMappingObject { get; set; } = new List<OutsideImportMappingObject>();
}
