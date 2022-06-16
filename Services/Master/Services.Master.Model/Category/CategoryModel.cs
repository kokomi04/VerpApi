﻿using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Master.Model.CategoryConfig;
using CategoryEntity = VErp.Infrastructure.EF.MasterDB.Category;

namespace VErp.Services.Master.Model.Category
{

    public class CategoryModel : CategoryListModel, IMapFrom<CategoryEntity>
    {
        public bool IsModule { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsOutSideData { get; set; }
        public bool IsTreeView { get; set; }
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
        public OutSideDataConfigModel OutSideDataConfig { get; set; }
        public bool? IsHide { get; set; }
    }

    public class CategoryFullModel : CategoryModel
    {
        public ICollection<CategoryFieldModel> CategoryField { get; set; }

        public CategoryFullModel()
        {
            CategoryField = new List<CategoryFieldModel>();
        }

    }
}
