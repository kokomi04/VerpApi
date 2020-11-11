﻿
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public OutSideDataConfigModel OutSideDataConfig { get; set; }
    }

    public class CategoryFullModel : CategoryModel
    {
        public CategoryFullModel()
        {
            CategoryFields = new List<CategoryFieldModel>();
        }
        public ICollection<CategoryFieldModel> CategoryFields { get; set; }
    }
}