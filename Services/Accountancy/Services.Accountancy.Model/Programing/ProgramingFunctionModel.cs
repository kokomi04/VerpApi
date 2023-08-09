using AutoMapper;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Programing
{
    public class ProgramingFunctionModel : ProgramingFunctionBaseModel, IMapFrom<ProgramingFunction>
    {
        protected void MappingBase<T>(Profile profile) where T : ProgramingFunctionBaseModel
            => profile.CreateMapCustom<T, ProgramingFunction>()
            .ForMember(d => d.Params, s => s.MapFrom(f => f.Params == null ? null : f.Params.JsonSerialize()))
            .ReverseMapCustom()
            .ForMember(d => d.Params, s => s.MapFrom(f => f.Params == null ? null : f.Params.JsonDeserialize<FunctionProgramParamType>()));
        public virtual void Mapping(Profile profile) => MappingBase<ProgramingFunctionModel>(profile);
    }

    public class ProgramingFunctionOutputList : ProgramingFunctionModel
    {
        public int ProgramingFunctionId { get; set; }
        public override void Mapping(Profile profile) => MappingBase<ProgramingFunctionOutputList>(profile);
    }

    public class FuncParameter
    {
        public EnumDataType DataType { get; set; }
        public object Value { get; set; }
    }

}
