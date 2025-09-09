using System;
using System.Collections.Generic;

namespace renjibackend.Models;

public partial class Accident
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
}
