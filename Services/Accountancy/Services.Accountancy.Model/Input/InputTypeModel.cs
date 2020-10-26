
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input

{
    public class InputTypeSimpleProjectMappingModel : InputTypeSimpleModel, IMapFrom<InputType>
    {
     
    }

    public class InputTypeModel: InputTypeSimpleProjectMappingModel
    {
        public InputTypeModel()
        {
        }

       
        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }

        //public MenuStyleModel MenuStyle { get; set; }
    }

    public class InputTypeFullModel : InputTypeModel
    {
        public InputTypeFullModel()
        {
            InputAreas = new List<InputAreaModel>();
        }
        public ICollection<InputAreaModel> InputAreas { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InputType, InputTypeFullModel>()
                .ForMember(dest => dest.InputAreas, opt => opt.MapFrom(src => src.InputArea));
        }
    }
}
