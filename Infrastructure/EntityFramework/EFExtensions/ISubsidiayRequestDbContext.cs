using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public interface ISubsidiayRequestDbContext
    {
        int SubsidiaryId { get; }
        ICurrentContextService CurrentContextService { get; }
    }
}
