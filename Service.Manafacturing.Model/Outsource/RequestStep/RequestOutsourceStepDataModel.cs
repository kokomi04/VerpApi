using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestDataModel : IMapFrom<OutsourceStepRequestData>
    {
        public long OutsourceStepRequestId { get; set; }
        [Required]
        public long ProductionStepLinkDataId { get; set; }
        [Required]
        public decimal? Quantity { get; set; }
        [Required]
        public int ProductionStepLinkDataRoleTypeId { get; set; }
    }
}
