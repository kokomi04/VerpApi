using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class MasterDBContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreated(modelBuilder);

            modelBuilder.Entity<User>().HasQueryFilter(o => !o.IsDeleted);

            modelBuilder.Entity<Employee>().HasQueryFilter(o => !o.IsDeleted);

        }
    }

   
}
