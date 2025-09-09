namespace renjibackend.DTO
{
    public class NewActionPlan
    {
        public class ActionPlanFormDto
        {
            public string ActionDescription { get; set; } = string.Empty;
            public int PersonInCharge { get; set; }
            public int Priority { get; set; }
            public int ActionTypes { get; set; }
            public DateTime TargetDate { get; set; }
        }

        public class ActionPlanDto
        {
            public ActionPlanFormDto Form { get; set; } = new ActionPlanFormDto();
            public int IncidentReportID { get; set; } = 0;
        }
    }
}
