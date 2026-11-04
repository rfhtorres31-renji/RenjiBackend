namespace renjibackend.DTO
{
    public class BarChart1Dto
    {
        public string y { get; set; }
        public int x { get; set; }
    }

    public class PieChartDto
    {
        public string label { get; set; }
        public int value { get; set; }
        public string percentage { get; set; }
    }

    public class ActionPlanDto
    {
        public string label { get; set; }
        public int value { get; set; }
        public string percentage { get; set; }
    }

    public class IncidentActionDto
    {
        public int ActionID { get; set; }
        public int IncidentReportID { get; set; }
        public string ActionDetail { get; set; }
        public string IncidentReportTitle { get; set; }
        public string Location { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string ActionType { get; set; }
        public string MaintenanceTeam { get; set; }
        public string AccidentType { get; set; }
        public string Status { get; set; }
    }
}
