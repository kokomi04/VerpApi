
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input

{
    public abstract class InputValueBillModel
    {
        public int InputTypeId { get; set; }
    }

    public class InputValueBillInputModel: InputValueBillModel, IMapFrom<InputValueBill>
    {
        public InputValueBillInputModel()
        {
            InputValueRows = new HashSet<InputValueRowInputModel>();
        }
        public ICollection<InputValueRowInputModel> InputValueRows { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueBillInputModel, InputValueBill>()
                .ForMember(dest => dest.InputValueRow, opt => opt.Ignore());
        }
    }

    public class InputValueBillOutputModel : InputValueBillModel, IMapFrom<InputValueBill>
    {
        public long InputValueBillId { get; set; }
        public InputValueBillOutputModel()
        {
            InputValueRows = new HashSet<InputValueRowOutputModel>();
        }
        public ICollection<InputValueRowOutputModel> InputValueRows { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueBill, InputValueBillOutputModel>()
                .ForMember(dest => dest.InputValueRows, opt => opt.MapFrom(src => src.InputValueRow));
        }
    }
}
