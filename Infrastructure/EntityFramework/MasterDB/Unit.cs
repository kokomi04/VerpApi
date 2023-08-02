﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class Unit
{
    public int UnitId { get; set; }

    public string UnitName { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public int UnitStatusId { get; set; }

    public int SubsidiaryId { get; set; }

    public int DecimalPlace { get; set; }
}
