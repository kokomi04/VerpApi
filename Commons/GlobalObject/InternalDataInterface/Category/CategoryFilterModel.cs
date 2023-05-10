using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Category
{
    public class CategoryFilterModel
    {
        public string Keyword { get; set; }
        public Clause ColumnsFilters { get; set; }
        public Dictionary<int, object> Filters { get; set; }        
        public NonCamelCaseDictionary FilterData { get; set; }
        public string ExtraFilter { get; set; }

        public ExtraFilterParam[] ExtraFilterParams { get; set; }

        public int Page { get; set; }
        public int Size { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
    }

    public class ExtraFilterParam
    {
        public string ParamName { get; set; }
        public EnumDataType DataType { get; set; }
        public object Value { get; set; }
    }

    public interface IDynamicCategoryHelper
    {
        Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames);
    }
}
