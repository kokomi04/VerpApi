using VErp.Commons.GlobalObject;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class MailTemplateSimpleModel
    {
        public int MailTemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}