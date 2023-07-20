using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.GlobalObject.Attributes
{
    public class UniqueAttribute<TContext, TEntity> : ValidationAttribute 
        where TContext : DbContext
        where TEntity : class
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var dbContext = (TContext)validationContext.GetService(typeof(TContext));

            var entityType = validationContext.ObjectType;
            var propertyName = validationContext.MemberName;
            var propertyValue = value.ToString();

            var set = dbContext.Set<TEntity>();

            if (set.Any(x => EF.Property<string>(x, propertyName) == propertyValue))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }

}
