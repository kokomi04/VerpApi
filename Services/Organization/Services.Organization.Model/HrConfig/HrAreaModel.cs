using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.HrConfig
{
    public class HrAreaInputModel : IMapFrom<HrArea>
    {
        public int HrAreaId { get; set; }
        public int HrTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên vùng dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tên vùng dữ liệu quá dài")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã vùng dữ liệu")]
        [MaxLength(45, ErrorMessage = "Vùng dữ liệu quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã vùng dữ liệu chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string HrAreaCode { get; set; }

        public bool IsMultiRow { get; set; }
        public bool IsAddition { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số lượng cột hiển thị")]
        public int Columns { get; set; }

        public string ColumnStyles { get; set; }
        public int SortOrder { get; set; }
    }

    public class HrAreaModel : HrAreaInputModel
    {
        public HrAreaModel()
        {
            HrAreaFields = new List<HrAreaFieldOutputFullModel>();
        }
        public ICollection<HrAreaFieldOutputFullModel> HrAreaFields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<HrArea, HrAreaModel>()
                .ForMember(dest => dest.HrAreaFields, opt => opt.MapFrom(src => src.HrAreaField));
        }
    }
}