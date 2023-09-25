using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleConfiguration
{
    public long ShiftScheduleId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public int ShiftScheduleMode { get; set; }

    public bool? IsMonday { get; set; }

    public bool? IsTuesday { get; set; }

    public bool? IsWednesday { get; set; }

    public bool? IsThursday { get; set; }

    public bool? IsFriday { get; set; }

    public bool? IsSaturday { get; set; }

    public bool? IsSunday { get; set; }

    public string ShiftAssignedDateJson { get; set; }

    [NotMapped]
    public List<DateTime?> ShiftAssignedDate
    {
        get { return JsonConvert.DeserializeObject<List<DateTime?>>(ShiftAssignedDateJson).ToList(); }
        set { ShiftAssignedDateJson = JsonConvert.SerializeObject(value.ToList()); }
    }

    public int? CycleRepeat { get; set; }

    public virtual ShiftSchedule ShiftSchedule { get; set; }
}
