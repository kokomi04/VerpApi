using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.EF.IdentityDB
{
    public class IdentityDBContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityDBContext(DbContextOptions options) : base(options)
        {

        }
    }
}
