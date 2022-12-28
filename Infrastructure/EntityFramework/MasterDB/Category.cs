﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Category
    {
        public Category()
        {
            CategoryField = new HashSet<CategoryField>();
            CategoryView = new HashSet<CategoryView>();
        }

        public int CategoryId { get; set; }
        public int? CategoryGroupId { get; set; }
        public string Title { get; set; }
        public string CategoryCode { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsTreeView { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsOutSideData { get; set; }
        public string UsePlace { get; set; }
        public int? MenuId { get; set; }
        public string ParentTitle { get; set; }
        public string DefaultOrder { get; set; }
        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
        public bool IsHide { get; set; }
        public string JoinSqlRaw { get; set; }
        public string SearchSqlRaw { get; set; }
        public string Key { get; set; }
        public string ParentKey { get; set; }

        public virtual CategoryGroup CategoryGroup { get; set; }
        public virtual ICollection<CategoryField> CategoryField { get; set; }
        public virtual ICollection<CategoryView> CategoryView { get; set; }
    }
}
