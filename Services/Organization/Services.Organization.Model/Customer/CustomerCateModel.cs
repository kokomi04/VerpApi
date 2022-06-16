using System.ComponentModel.DataAnnotations;
using Verp.Resources.Organization.Customer;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerCateModel : IMapFrom<CustomerCate>
    {
        public int? CustomerCateId { get; set; }
        [Required(ErrorMessageResourceType = typeof(CustomerValidationMessage), ErrorMessageResourceName = nameof(CustomerValidationMessage.CustomerCateCodeTooShort))]
        [MinLength(1, ErrorMessageResourceType = typeof(CustomerValidationMessage), ErrorMessageResourceName = nameof(CustomerValidationMessage.CustomerCateCodeTooShort))]
        [MaxLength(128, ErrorMessageResourceType = typeof(CustomerValidationMessage), ErrorMessageResourceName = nameof(CustomerValidationMessage.CustomerCateCodeTooLong))]
        public string CustomerCateCode { get; set; }
        [Required(ErrorMessageResourceType = typeof(CustomerValidationMessage), ErrorMessageResourceName = nameof(CustomerValidationMessage.CustomerCateNameTooShort))]
        [MinLength(1, ErrorMessageResourceType = typeof(CustomerValidationMessage), ErrorMessageResourceName = nameof(CustomerValidationMessage.CustomerCateNameTooShort))]
        [MaxLength(128, ErrorMessageResourceType = typeof(CustomerValidationMessage), ErrorMessageResourceName = nameof(CustomerValidationMessage.CustomerCateNameTooLong))]
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
}
