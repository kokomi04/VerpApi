
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Model.Category
{
    public class CategoryAreaInputModel : IMapFrom<CategoryArea>
    {
        public int CategoryAreaId { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên vùng dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tên vùng dữ liệu quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã vùng dữ liệu")]
        [MaxLength(45, ErrorMessage = "Vùng dữ liệu quá dài")]
        public string CategoryAreaCode { get; set; }
        public int SortOrder { get; set; }
        public EnumCategoryAreaType CategoryAreaType { get; set; }
    }

    public class CategoryAreaModel : CategoryAreaInputModel
    {
        public CategoryAreaModel()
        {
            CategoryFields = new List<CategoryFieldOutputModel>();
        }
        public ICollection<CategoryFieldOutputModel> CategoryFields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryArea, CategoryAreaModel>().ForMember(dest => dest.CategoryFields, opt => opt.MapFrom(src => src.CategoryField));
        }
    }

}
