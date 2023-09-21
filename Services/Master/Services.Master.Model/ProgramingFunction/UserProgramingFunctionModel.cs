using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.ProgramingFunction
{
    public class UserProgramingFunctionModel : UserProgramingFuctionModel, IMapFrom<UserProgramingFunction>
    {
        protected void MappingBase<T>(Profile profile) where T : UserProgramingFuctionModel
           => profile.CreateMapCustom<T, UserProgramingFunction>()
           .ForMember(d => d.Params, s => s.MapFrom(f => f.Params == null ? null : f.Params.JsonSerialize()))
           .ReverseMapCustom()
           .ForMember(d => d.Params, s => s.MapFrom(f => f.Params == null ? null : f.Params.JsonDeserialize<FunctionProgramParamType>()));
        public virtual void Mapping(Profile profile) => MappingBase<UserProgramingFunctionModel>(profile);
    }
    public class UserProgramingFunctionOutputList : UserProgramingFunctionModel
    {
        public int ProgramingFunctionId { get; set; }
        public override void Mapping(Profile profile) => MappingBase<UserProgramingFunctionOutputList>(profile);
    }

    public class FuncParameter
    {
        public EnumDataType DataType { get; set; }
        public object Value { get; set; }
    }
}
