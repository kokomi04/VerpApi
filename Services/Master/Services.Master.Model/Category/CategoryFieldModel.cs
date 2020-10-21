using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Category
{
    public class CategoryFieldModel : IMapFrom<CategoryField>
    {
        public int CategoryFieldId { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên trường dữ liệu")]
        [MaxLength(45, ErrorMessage = "Tên trường dữ liệu quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Tên trường dữ liệu chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string CategoryFieldName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool AutoIncrement { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public bool IsShowList { get; set; }
        public bool IsShowSearchTable { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public bool IsTreeViewKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsReadOnly { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public int DecimalPlace { get; set; }
        public string DefaultValue { get; set; }

        public bool Compare(CategoryField curField)
        {
            return !curField.IsDeleted &&
                CategoryId == curField.CategoryId &&
                CategoryFieldName == curField.CategoryFieldName &&
                Title == curField.Title &&
                SortOrder == curField.SortOrder &&
                DataTypeId == curField.DataTypeId &&
                DataSize == curField.DataSize &&
                FormTypeId == curField.FormTypeId &&
                AutoIncrement == curField.AutoIncrement &&
                IsRequired == curField.IsRequired &&
                IsUnique == curField.IsUnique &&
                IsHidden == curField.IsHidden &&
                IsShowList == curField.IsShowList &&
                IsShowSearchTable == curField.IsShowSearchTable &&
                RegularExpression == curField.RegularExpression &&
                Filters == curField.Filters &&
                IsTreeViewKey == curField.IsTreeViewKey &&
                IsReadOnly == curField.IsReadOnly &&
                RefTableCode == curField.RefTableCode &&
                RefTableField == curField.RefTableField &&
                RefTableTitle == curField.RefTableTitle &&
                DecimalPlace == curField.DecimalPlace &&
                DefaultValue == curField.DefaultValue;

        }
    }

    public class CategoryFieldReferModel
    {
        public string CategoryCode { get; set; }
        public string CategoryFieldName { get; set; }
        public string Title { get; set; }
    }

    public class ReferInputModel
    {
        public IList<string> CategoryCodes { get; set; }
        public IList<string> FieldNames { get; set; }

        public ReferInputModel()
        {
            CategoryCodes = new List<string>();
            FieldNames = new List<string>();
        }
    }
}
