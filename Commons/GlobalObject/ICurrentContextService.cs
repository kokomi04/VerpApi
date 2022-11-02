using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject
{
    public interface ICurrentContextService
    {
        string TraceIdentifier { get; }
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
        int ModuleId { get; }
    }

    public class ScopeCurrentContextService : ICurrentContextService
    {
        public ScopeCurrentContextService(ICurrentContextService currentContextService)
        : this(
              currentContextService.TraceIdentifier,
                currentContextService.UserId,
                currentContextService.Action,
                currentContextService.RoleInfo,
                currentContextService.StockIds,
                currentContextService.SubsidiaryId,
                currentContextService.TimeZoneOffset,
                currentContextService.Language,
                currentContextService.IpAddress,
                currentContextService.Domain
        )
        {

        }

        public ScopeCurrentContextService(string traceIdentifier, int userId, EnumActionType action, RoleInfo roleInfo, IList<int> stockIds, int subsidiaryId, int? timeZoneOffset, string language, string ipAddress, string domain)
        {
            TraceIdentifier = traceIdentifier;
            UserId = userId;
            SubsidiaryId = subsidiaryId;
            Action = action;
            RoleInfo = roleInfo == null ? null : new RoleInfo(roleInfo.RoleId, roleInfo.ChildrenRoleIds?.Select(c => c)?.ToList(), roleInfo.IsModulePermissionInherit, roleInfo.IsDataPermissionInheritOnStock, roleInfo.RoleName);
            StockIds = stockIds == null ? null : stockIds.Select(s => s).ToList();
            TimeZoneOffset = timeZoneOffset;
            Language = language;
            IpAddress = ipAddress;
            Domain = domain;
        }

        public void SetSubsidiaryId(int subsidiaryId)
        {
            SubsidiaryId = subsidiaryId;
        }
        public string TraceIdentifier { get; }
        public int UserId { get; } = 0;
        public int SubsidiaryId { get; private set; } = 0;
        public EnumActionType Action { get; }
        public IList<int> StockIds { get; }
        public RoleInfo RoleInfo { get; }
        public int? TimeZoneOffset { get; }
        public bool IsDeveloper { get; } = false;
        public string Language { get; }
        public string IpAddress { get; }
        public string Domain { get; }
        public int ModuleId { get; }
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
