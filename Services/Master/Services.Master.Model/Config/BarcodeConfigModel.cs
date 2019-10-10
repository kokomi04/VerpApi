using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Config
{
    public class BarcodeConfigModel
    {       
        public EnumBarcodeStandard BarcodeStandardId { get; set; }
        public string Name { get; set; }
        public bool IsActived { get; set; }
        public BarcodeConfigEan8 Ean8 { get; set; }
        public BarcodeConfigEan13 Ean13 { get; set; }
    }
}
