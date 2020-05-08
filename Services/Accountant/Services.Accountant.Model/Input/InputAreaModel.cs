using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputAreaInputModel : IMapFrom<InputArea>
    {
        public int InputAreaId { get; set; }
        public int InputTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên vùng dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tên vùng dữ liệu quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã vùng dữ liệu")]
        [MaxLength(45, ErrorMessage = "Vùng dữ liệu quá dài")]
        public string InputAreaCode { get; set; }
        public bool IsMultiRow { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng cột hiển thị")]
        public int Columns { get; set; }
    }

    public class InputAreaOutputModel : InputAreaInputModel
    {
        public InputAreaOutputModel()
        {
            InputAreaFields = new List<InputAreaFieldOutputFullModel>();
        }
        public ICollection<InputAreaFieldOutputFullModel> InputAreaFields { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputArea, InputAreaOutputModel>()
                .ForMember(dest => dest.InputAreaFields, opt => opt.MapFrom(src => src.InputAreaField));
        }
    }
}
