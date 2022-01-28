using VErp.Commons.GlobalObject;

namespace VErp.Services.Master.Model.Category
{
    public class MapObjectInputModel
    {
        public string CategoryCode { get; set; }
        public string CategoryTitle { get; set; }
        public string CategoryFieldName { get; set; }
        public string Value { get; set; }
        public string Filters { get; set; }
    }

    public class MapObjectOutputModel : MapObjectInputModel
    {
        public NonCamelCaseDictionary ReferObject { get; set; }
    }
}
