
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Model.Category

{
    public abstract class CategoryBase<T> where T : CategoryBase<T>
    {
        protected CategoryBase()
        {
            SubCategories = new List<T>();
        }
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [MaxLength(256, ErrorMessage = "Tên danh mục quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã danh mục")]
        [MaxLength(45, ErrorMessage = "Mã danh mục quá dài")]
        public string CategoryCode { get; set; }
        public bool IsModule { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsOutSideData { get; set; }
        public bool IsTreeView { get; set; }
        public OutSideDataConfigModel OutSideDataConfig { get; set; }
        public ICollection<T> SubCategories { get; set; }
    }

    public class CategoryModel : CategoryBase<CategoryModel>, IMapFrom<CategoryEntity>
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryEntity, CategoryModel>();
            profile.CreateMap<CategoryModel, CategoryEntity>().ForMember(c => c.InverseParent, act => act.Ignore());
        }
    }

    public class CategoryFullModel : CategoryBase<CategoryFullModel>, IMapFrom<CategoryEntity>
    {
        public CategoryFullModel()
        {
            CategoryFields = new List<CategoryFieldOutputModel>();
        }

        public ICollection<CategoryFieldOutputModel> CategoryFields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryEntity, CategoryFullModel>().ForMember(dest => dest.CategoryFields, opt => opt.MapFrom(src => src.CategoryField));
        }
    }
}
