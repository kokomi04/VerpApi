using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Category
{
  
    public class CategoryGroupModel : IMapFrom<CategoryGroup>
    {
        public int CategoryGroupId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên nhóm danh mục")]
        [MaxLength(256, ErrorMessage = "Tên nhóm danh mục quá dài")]
        public string CategoryGroupName { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
}
