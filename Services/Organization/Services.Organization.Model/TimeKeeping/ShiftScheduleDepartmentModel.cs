using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleDepartmentModel : IMapFrom<ShiftScheduleDepartment>
{
    public long ShiftScheduleId { get; set; }

    public int DepartmentId { get; set; }
}
