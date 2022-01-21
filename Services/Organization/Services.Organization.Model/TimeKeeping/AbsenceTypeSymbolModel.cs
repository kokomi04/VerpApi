using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class AbsenceTypeSymbolModel : IMapFrom<AbsenceTypeSymbol>
    {
        public int AbsenceTypeSymbolId { get; set; }
        public string TypeSymbolCode { get; set; }
        public string TypeSymbolDescription { get; set; }
        public string SymbolCode { get; set; }
        public bool IsUsed { get; set; }
        public bool IsCounted { get; set; }
    }
}