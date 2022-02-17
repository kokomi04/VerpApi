using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class CountedSymbolModel : IMapFrom<CountedSymbol>
    {
        public int CountedSymbolId { get; set; }
        public int CountedSymbolType { get; set; }
        public string SymbolCode { get; set; }
        public string SymbolDescription { get; set; }
        public bool IsHide { get; set; }
    }
}