using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Config
{
    public class BarcodeConfigModel
    {
        public EnumBarcodeStandard BarcodeStandardId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình")]
        [MaxLength(128, ErrorMessage = "Tên cấu hình quá dài")]
        public string Name { get; set; }
        public bool IsActived { get; set; }
        public BarcodeConfigEan8 Ean8 { get; set; }
        public BarcodeConfigEan13 Ean13 { get; set; }
    }
}
