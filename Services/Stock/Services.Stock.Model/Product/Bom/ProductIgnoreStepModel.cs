
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductIgnoreStepModel : ProductBomInfoPathBaseModel
    {
        public long ProductIgnoreStepId { get; set; }

        [Required(ErrorMessage = "Chi tiết nguyên vật liệu không hợp lệ")]
        public override int ProductId { get; set; }
    }

    public class ProductBomIgnoreStepUpdateInfo
    {
        public ProductBomIgnoreStepUpdateInfo()
        {

        }
        public ProductBomIgnoreStepUpdateInfo(IList<ProductIgnoreStepModel> bomIgnoreSteps, bool cleanOldData)
        {
            BomIgnoreSteps = bomIgnoreSteps;
            CleanOldData = cleanOldData;
        }
        public IList<ProductIgnoreStepModel> BomIgnoreSteps { get; set; }
        public bool CleanOldData { get; set; }
    }
}