using System;
using System.Collections.Generic;

namespace renjibackend.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int DepartmentId { get; set; }

    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
}
