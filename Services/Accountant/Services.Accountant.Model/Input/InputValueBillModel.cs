
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input
{
    public abstract class InputValueBillModel<T> where T : InputValueRowInputModel
    {
        public InputValueBillModel()
        {
            InputValueRows = new HashSet<T>();
        }
        public int InputTypeId { get; set; }
        public ICollection<T> InputValueRows { get; set; }
    }

    public class InputValueBillInputModel : InputValueBillModel<InputValueRowInputModel>, IMapFrom<InputValueBill>
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueBillInputModel, InputValueBill>()
                .ForMember(dest => dest.InputValueRow, opt => opt.Ignore());
        }
    }

    public class InputValueBillOutputModel : InputValueBillModel<InputValueRowOutputModel>, IMapFrom<InputValueBill>
    {
        public long InputValueBillId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueBill, InputValueBillOutputModel>()
                .ForMember(dest => dest.InputValueRows, opt => opt.MapFrom(src => src.InputValueRow));
        }
    }
}
