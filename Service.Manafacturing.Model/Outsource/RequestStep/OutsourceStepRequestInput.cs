using System;
using System.Collections.Generic;
using System.Text;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestInput
    {
        public long ProductionOrderId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public ProductionProcessOutsourceStep ProductionProcessOutsource { get; set; }
        public OutsourceStepSetting Setting { get; set; }
    }

    public class OutsourceStepRequestDetailInput
    {
        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class OutsourceStepSetting
    {
        public string Mode { get; set; }
        public ZoomSetting Zoom { get; set; }
        public IList<NodeSetting> Nodes {get;set;}
    }

    public class ZoomSetting
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Scale { get; set; }
    }

    public class NodeSetting
    {
        public string Code { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
