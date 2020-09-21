using System;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject
{
    public interface ICurrentContextService
    {
        int UserId { get; }
        int SubsidiaryId { get; }
        EnumAction Action { get; }
        IList<int> StockIds { get; }
        RoleInfo RoleInfo { get; }
    }
}
