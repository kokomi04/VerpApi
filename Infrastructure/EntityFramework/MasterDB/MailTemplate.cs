using System;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class MailTemplate
    {
        public int MailTemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
