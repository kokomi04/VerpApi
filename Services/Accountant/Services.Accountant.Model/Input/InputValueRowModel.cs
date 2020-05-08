
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputValueRowInputModel: IMapFrom<InputValueRow>
    {
        public int InputAreaId { get; set; }
        public long InputValueRowId { get; set; }
        public InputValueRowVersionInputModel InputValueRowVersion { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueRowInputModel, InputValueRow>()
                .ForMember(dest => dest.InputValueRowVersion, opt => opt.Ignore());
        }
    }

    public class InputValueRowOutputModel : InputValueRowInputModel 
    {
        public long InputValueBillId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public new void Mapping(Profile profile)
        {
            profile.CreateMap<InputValueRow, InputValueRowOutputModel>()
                .ForMember(dest => dest.InputValueRowVersion, opt => opt.MapFrom(src => src.InputValueRowVersion.FirstOrDefault(rv => rv.InputValueRowVersionId == src.LastestInputValueRowVersionId)));
        }
    }

}
