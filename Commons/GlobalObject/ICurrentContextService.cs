﻿using System;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject
{
    public interface ICurrentContextService
    {
        int UserId { get; }
        int SubsidiaryId { get; }
        EnumActionType Action { get; }
        IList<int> StockIds { get; }
        RoleInfo RoleInfo { get; }
        int? TimeZoneOffset { get; }
        bool IsDeveloper { get; }
        string Language { get; }
        string IpAddress { get; }
        string Domain { get; }
        int ModuleId {get;}
    }

    public static class CurrentContextServiceExtensions
    {
        public static DateTime GetNowUtc(this ICurrentContextService currentContextService)
        {
            return DateTime.UtcNow;
        }
        public static DateTime GetNowInTimeZone(this ICurrentContextService currentContextService)
        {
            if (currentContextService.TimeZoneOffset.HasValue)
            {
                return DateTime.UtcNow.AddMinutes(-currentContextService.TimeZoneOffset.Value);
            }
            else
            {
                return DateTime.UtcNow.AddMinutes(420);
            }
        }
    }
}
