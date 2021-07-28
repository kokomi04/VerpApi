using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Dictionary
{
    public class ReuseContentModel : IMapFrom<ReuseContent>
    {
        private string _title;


        public long? ReuseContentId { get; set; }

        [MaxLength(128, ErrorMessage = "Key quá dài")]
        public string Key { get; set; }

        [MaxLength(128, ErrorMessage = "Tiêu đề quá dài")]
        public string Title
        {
            get { if (string.IsNullOrWhiteSpace(_title)) return Utils.SubStringMaxLength(Content, 64, true, true); return _title; }
            set { _title = value?.Trim(); }
        }

        [MaxLength(4000, ErrorMessage = "Nội dung quá dài")]
        public string Content { get; set; }
    }
}
