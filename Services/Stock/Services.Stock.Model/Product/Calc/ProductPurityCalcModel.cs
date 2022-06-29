using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product.Calc
{
    public class ProductPurityCalcModel : IMapFrom<ProductPurityCalc>
    {
        public int? ProductPurityCalcId { get; set; }
        [MaxLength(128)]
        public string Title { get; set; }
        [MaxLength(1024)]
        public string Description { get; set; }
        public string EvalSourceCodeJs { get; set; }
    }
}
