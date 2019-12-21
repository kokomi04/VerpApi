using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Unit
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }        
    }

    public class UnitModel: Unit
    {
        public string ProductCode { set; get; }
    }
}
