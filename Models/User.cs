using System;
using System.Collections.Generic;

namespace renjibackend.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int DepartmentId { get; set; }

    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
}
