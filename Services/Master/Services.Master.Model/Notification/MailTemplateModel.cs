using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Notification
{
    public class MailTemplateModel : IMapFrom<MailTemplate>
    {
        public int MailTemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}