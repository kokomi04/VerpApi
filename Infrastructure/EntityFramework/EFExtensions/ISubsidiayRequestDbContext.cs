using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public interface ISubsidiayRequestDbContext
    {       
        int SubsidiaryId { get; }
    }
}
