using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class AbsenceTypeSymbolModel : IMapFrom<AbsenceTypeSymbol>
    {
        public int AbsenceTypeSymbolId { get; set; }
        public int NumericalOrder { get; set; }
        public string TypeSymbolDescription { get; set; }
        public int MaxOfDaysOffPerMonth { get; set; }
        public string SymbolCode { get; set; }
        public bool IsUsed { get; set; }
        public bool IsCounted { get; set; }
        public double SalaryRate { get; set; }
    }
}