using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Service.Activity
{
    public interface ICurrentContextService
    {
        int UserId { get; }
        EnumAction Action { get; }
    }
}
