using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.System
{
    public class BarcodeConfigListOutput
    {
        public int BarcodeConfigId { get; set; }
        public string Name { get; set; }
        public EnumBarcodeStandard BarcodeStandardId { get; set; }
        public bool IsActived { get; set; }
    }
}
