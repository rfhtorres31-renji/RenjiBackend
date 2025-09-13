using System;
using System.Collections.Generic;

namespace renjibackend.Models;

public partial class ActionPlan
{
    public int Id { get; set; }

    public int IncidentReportId { get; set; }

    public int MaintenanceStaffId { get; set; }

    public string ActionDetail { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? Priority { get; set; }

    public int? ActionType { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CompletedDate { get; set; }

    public bool? IsOverDueNotified { get; set; }

    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();

    public virtual MaintenanceTeam MaintenanceStaff { get; set; } = null!;
}
