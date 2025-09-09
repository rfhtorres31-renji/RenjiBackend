using System;
using System.Collections.Generic;

namespace renjibackend.Models;

public partial class MaintenanceTeam
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactNumber { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ActionPlan> ActionPlans { get; set; } = new List<ActionPlan>();
}
