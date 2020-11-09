using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartDetailModel: IMapFrom<RequestOutsourcePartDetail>
    {
        public int RequestOutsourcePartDetailId { get; set; }
        public int RequestOutsourcePartId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public int Quanity { get; set; }
    }
}
