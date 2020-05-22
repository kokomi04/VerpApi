using AutoMapper;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category
{
    public class CategoryFieldInputModel : IMapFrom<CategoryField>
    {
        public int CategoryFieldId { get; set; }
        public int CategoryAreaId { get; set; }
        public int CategoryId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên trường dữ liệu")]
        [MaxLength(45, ErrorMessage = "Tên trường dữ liệu quá dài")]
        public string CategoryFieldName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool AutoIncrement { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public bool IsShowList { get; set; }
        public bool IsShowSearchTable { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public bool IsTreeViewKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsReadOnly { get; set; }
    }


    public class CategoryFieldOutputModel : CategoryFieldInputModel
    {
        public int? ReferenceCategoryId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryField, CategoryFieldOutputModel>()
                .ForMember(dest => dest.ReferenceCategoryId, opt => opt.MapFrom(src => src.ReferenceCategoryField.CategoryId));
        }
    }
}
