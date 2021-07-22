using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomOutputBase
    {
        public long? ProductBomId { get; set; }
        public int Level { get; set; }
        public int ProductId { get; set; }
        public int? ChildProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }
        public string Specification { get; set; }

        public decimal Quantity { get; set; }
        public decimal Wastage { get; set; }
        public decimal TotalQuantity { get; set; }

        public string Description { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }
        public bool IsMaterial { get; set; }
        public string NumberOrder { get; set; }
        public int ProductUnitConversionId { get; set; }
        public int DecimalPlace { get; set; }
        public int? InputStepId { get; set; }
        public int? OutputStepId { get; set; }
     
    }


    public class ProductBomPreviewOutput: ProductBomOutputBase
    {
      

        public IList<int> PropertyIds { get; set; }

        public int[] PathProductIds { get; set; }
    }

    public class ProductBomEntity : ProductBomOutputBase
    {
        public string PathProductIds { get; set; }
    }

    public class ProductBomOutput : ProductBomOutputBase, IMapFrom<ProductBomEntity>
    {
        public int[] PathProductIds { get; set; }
        public IList<long> Properties { get; set; }

        public ProductBomOutput()
        {
            Properties = new List<long>();
        }
        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<ProductBomEntity, ProductBomOutput>()
                .ForMember(m => m.PathProductIds, v => v.Ignore());
        }
    }

    public class ProductBomImportModel
    {
        const string MainProductGroup = "Mặt hàng chính";
        const string ChildProductGroup = "Chi tiết";

        [Display(Name = "Mã mặt hàng", GroupName = MainProductGroup)]
        [Required(ErrorMessage = "Vui lòng nhập mã mặt hàng chính")]
        public string ProductCode { get; set; }

        [Display(Name = "Tên mặt hàng (Nếu có)", GroupName = MainProductGroup)]
        public string ProductName { get; set; }

        [Display(Name = "Đơn vị mặt hàng (Nếu có)", GroupName = MainProductGroup)]
        public string UnitName { get; set; }

        [Display(Name = "Định danh loại mã mặt hàng (Nếu có)", GroupName = MainProductGroup)]
        public string ProductTypeCode { get; set; }

        [Display(Name = "Quy cách mặt hàng (Nếu có)", GroupName = MainProductGroup)]
        public string Specification { get; set; }



        [Display(Name = "Danh mục mặt hàng (Nếu có)", GroupName = MainProductGroup)]
        public string ProductCateName { get; set; }

        [Display(Name = "Mã chi tiết", GroupName = ChildProductGroup)]
        [Required(ErrorMessage = "Vui lòng nhập mã chi tiết")]
        public string ChildProductCode { get; set; }

        [Display(Name = "Tên chi tiết", GroupName = ChildProductGroup)]
        public string ChildProductName { get; set; }

        [Display(Name = "Đơn vị chi tiết", GroupName = ChildProductGroup)]
        public string ChildUnitName { get; set; }

        [Display(Name = "Định danh loại mặt hàng của chi tiết (Nếu có)", GroupName = ChildProductGroup)]
        public string ChildProductTypeCode { get; set; }
        [Display(Name = "Danh mục chi tiết (Nếu có)", GroupName = ChildProductGroup)]
        public string ChildProductCateName { get; set; }

        [Display(Name = "Quy cách mặt hàng (Nếu có)", GroupName = ChildProductGroup)]
        public string ChildSpecification { get; set; }



        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Tỷ lệ hao hụt (Mặc định = 1)")]
        public decimal? Wastage { get; set; }

        [Display(Name = "Đánh dấu là nguyên liệu đầu vào (Có, Không)")]
        public bool IsMaterial { get; set; }

        //[Display(Name = "Thuộc công đoạn đầu vào nào?")]
        //public int? InputStepId { get; set; }

        [Display(Name = "Thuộc công đoạn vào nào?", GroupName = ChildProductGroup)]
        public string InputStepName { get; set; }

        //[Display(Name = "Thuộc công đoạn đầu ra nào?")]
        //public int? OutputStepId { get; set; }

        [Display(Name = "Thuộc công đoạn ra nào?", GroupName = ChildProductGroup)]
        public string OutputStepName { get; set; }


        [FieldDataIgnore]
        public ISet<int> Properties { get; set; }
    }

    public class ProductBomByProduct
    {
        public ProductRootBomInfo Info { get; set; }
        public IList<ProductBomPreviewOutput> Boms { get; set; }
    }


    public class ProductRootBomInfo
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductCateName { get; set; }
        public string Specification { get; set; }
    }
}
