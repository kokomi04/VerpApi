
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryModel : IMapFrom<CategoryEntity>
    {
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [MaxLength(256, ErrorMessage = "Tên danh mục quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã danh mục")]
        [MaxLength(45, ErrorMessage = "Mã danh mục quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã danh mục chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string CategoryCode { get; set; }
        public bool IsModule { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsOutSideData { get; set; }
        public bool IsTreeView { get; set; }
        public OutSideDataConfigModel OutSideDataConfig { get; set; }
    }

    public class CategoryFullModel : CategoryModel
    {
        public CategoryFullModel()
        {
            CategoryAreas = new List<CategoryAreaModel>();
        }
      
        public ICollection<CategoryAreaModel> CategoryAreas { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryEntity, CategoryFullModel>().ForMember(dest => dest.CategoryAreas, opt => opt.MapFrom(src => src.CategoryArea));
        }
    }
}
