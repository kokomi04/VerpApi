
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input

{
    public abstract class InputValueRowModel
    {
        protected InputValueRowModel()
        {
        }
        public int InputAreaId { get; set; }
        public long InputValueRowId { get; set; }
    }

    public class InputValueRowInputModel: InputValueRowModel, IMapFrom<InputValueRow>
    {
        public InputValueRowInputModel()
        {
        }
        public InputValueRowVersionInputModel InputValueRowVersion { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueRowInputModel, InputValueRow>()
                .ForMember(dest => dest.InputValueRowVersion, opt => opt.Ignore());
        }
    }

    public class InputValueRowOutputModel : InputValueRowModel, IMapFrom<InputValueRow>
    {
        public InputValueRowOutputModel()
        {
            InputValueRowVersions = new HashSet<InputValueRowVersionOutputModel>();
        }
        public long InputValueBillId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public ICollection<InputValueRowVersionOutputModel> InputValueRowVersions { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueRow, InputValueRowOutputModel>()
                .ForMember(dest => dest.InputValueRowVersions, opt => opt.MapFrom(src => src.InputValueRowVersion));
        }
    }

}
