using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageCustomPropertyModel : IMapFrom<PackageCustomProperty>
    {
        public int? PackageCustomPropertyId { get; set; }
        public EnumDataType DataTypeId { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
