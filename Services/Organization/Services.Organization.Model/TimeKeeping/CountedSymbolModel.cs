using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class CountedSymbolModel : IMapFrom<CountedSymbol>
    {
        public int CountedSymbolId { get; set; }
        public EnumCountedSymbol CountedSymbolType { get; set; }
        public string SymbolCode { get; set; }
        public string SymbolDescription { get; set; }
        public int CountedPriority { get; set; }
        public bool IsHide { get; set; }
    }
}