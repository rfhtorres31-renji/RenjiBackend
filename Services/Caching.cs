using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using renjibackend.Data;


namespace renjibackend.Services
{
    public class Caching
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


        private readonly IRSDbContext db;
        private readonly IMemoryCache cache;

        public Caching(IMemoryCache _cache, IRSDbContext _db)
        {
            this.db = _db;
            this.cache = _cache;
        }

        // Controller for KPI Computations and Incident Reports List
        public async Task<object> GetIncidentReportsListCaching()
        {

            if (cache.TryGetValue("reports", out List<object> cachedData))
            {
                return cachedData;
            }

            using (var dbContext = new IRSDbContext())
            {
                var reports = await db.IncidentReports.Select(n => new {
                    ID = n.Id,
                    Type = db.Accidents.Where(u => u.Id == n.AccidentId).Select(n => n.Name).FirstOrDefault(),
                    Description = n.Description,
                    Location = n.Location,
                    ReportedDate = n.ReportedDate,
                    ReportedBy = db.Users.Where(u => u.Id == n.ReportedBy).Select(n => n.FirstName + ' ' + n.LastName).FirstOrDefault(),
                    Status = n.Status == 10 ? "Open" :
                     n.Status == 20 ? "In Progress" :
                     n.Status == 30 ? "Resolved" : ""
                }).AsNoTracking().ToListAsync();

                int noOfOpenReports = await db.IncidentReports.Where(u => u.Status == 10).CountAsync();
                int noOfInProgressReports = await db.IncidentReports.Where(u => u.Status == 20).CountAsync();
                int noOfResolvedReports = await db.IncidentReports.Where(u => u.Status == 30).CountAsync();

                var reportCounts = new { open = noOfOpenReports, inProgress = noOfInProgressReports, resolved = noOfResolvedReports };

                var reportsObj = new { data = reports, reportCounts };

                cache.Set("reports", reportsObj, TimeSpan.FromSeconds(30));

                return reportsObj;
            }

        }


        public async Task<object> GetTopIncidentReportsByUsers()
        {
            if (cache.TryGetValue("topIncidentReportsByUsers", out List<object> cachedData))
            {
                return cachedData;
            }

            using (IRSDbContext db = new IRSDbContext())
            {

                var query = await (from ir in db.IncidentReports
                                   join u in db.Users on ir.ReportedBy equals u.Id
                                   group ir by u.Id into g
                                   select new
                                   {
                                       Name = db.Users.Where(u => u.Id == g.Key).Select(n =>
                                          n.FirstName + " " + n.LastName + ", " + db.Departments.Where(u => u.Id == n.DepartmentId).Select(n => n.Name).FirstOrDefault()

                                       ).FirstOrDefault(),
                                       Count = g.Count()
                                   }
                                  ).OrderByDescending(o => o.Count).ToListAsync();


                cache.Set("topIncidentReportsByUsers", query, TimeSpan.FromSeconds(30));
                return query;

            }

        }


        // API Function Controller for Getting Top Users by how many accident they report
        public async Task<object> GetTopUsersbyReports()
        {
            DateTime now = DateTime.Now;

            int month = now.Month;
            int year = now.Year;

            if (cache.TryGetValue("getTopUsersbyReports", out List<object> cachedData))
            {
                return cachedData;
            }


            using (IRSDbContext db = new IRSDbContext())
            {

                var query = await (from ir in db.IncidentReports
                                  join u in db.Users on ir.ReportedBy equals u.Id
                                  where ir.ReportedDate.Month == month && ir.ReportedDate.Year == year
                                  group ir by u.Id into g
                                  select new
                                  {
                                      Name = db.Users.Where(u => u.Id == g.Key).Select(n =>
                                         n.FirstName + " " + n.LastName + ", " + db.Departments.Where(u => u.Id == n.DepartmentId).Select(n => n.Name).FirstOrDefault()
                                       ).FirstOrDefault(),
                                      Count = g.Count()
                                  }
                                  ).OrderByDescending(o => o.Count).ToListAsync();

                cache.Set("getTopUsersbyReports", query, TimeSpan.FromSeconds(30));
                return query;
            }
        }


        public async Task<object> GetSummaryReportsBarChart_1Caching()
        {

            if (cache.TryGetValue("barChart", out List<BarChart1Dto> cachedData))
            {
                return cachedData;
            }

            using (var dbContext = new IRSDbContext())
            {
                var query1 = from ir in dbContext.IncidentReports
                             join a in dbContext.Accidents
                             on ir.AccidentId equals a.Id
                             group ir by a.Name into g
                             select new BarChart1Dto
                             {
                                 y = g.Key, // Accident Types
                                 x = g.Count() // Total per Accident Types
                             };

                var result1 = await query1.OrderByDescending(x => x.y).ToListAsync();
                cache.Set("barChart", result1, TimeSpan.FromSeconds(30));
                return result1;
            }

        }


        public async Task<object> GetSummaryReportsPieChart_2Caching()
        {

            if (cache.TryGetValue("pieChart", out List<PieChartDto> cachedData))
            {
                return cachedData;
            }

            using (IRSDbContext db = new IRSDbContext())
            {
                var totalCount = db.IncidentReports.Count();

                var query = await (from ir in db.IncidentReports
                             join dep in db.Departments
                             on ir.DepartmentId equals dep.Id
                             group ir by dep.Id into g
                             select new 
                             {
                                 label = db.Departments.Where(u => u.Id == g.Key).Select(n => n.Name).FirstOrDefault(),
                                 value = g.Count(),
                                 percentage = ((double)g.Count() / totalCount * 100).ToString("0.0") + "%"
                             }).ToListAsync();


                cache.Set("pieChart", query, TimeSpan.FromSeconds(30));

                return query;
            }
        }


        public async Task<object> GetActionPlanChartDonutChart()
        {

            if (cache.TryGetValue("actionPlanDonutChart", out List<object> cachedData))
            {
                return cachedData;
            }


            using (var db = new IRSDbContext())
            {
                var pendingActionPlans = await db.IncidentReports
                                  .Include(i => i.ActionPlan)
                                  .Include(i => i.Department)
                                  .Include(i => i.Accident)
                                  .Where(u => u.ActionPlan.Status != 30).ToListAsync();

                int totalPendingPlans = pendingActionPlans.Count();

                var completedActionPlans = await db.IncidentReports
                                              .Include(i => i.ActionPlan)
                                              .Include(i => i.Department)
                                              .Include(i => i.Accident)
                                              .Where(u => u.ActionPlan.Status == 30).ToListAsync();


                int totalCompletedPlans = completedActionPlans.Count();


                int totalActionPlans = db.IncidentReports
                                      .Include(i => i.ActionPlan)
                                      .Include(i => i.Department)
                                      .Include(i => i.Accident).Count();



                double percentagePendingPlans = Math.Round(((double)totalPendingPlans / totalActionPlans) * 100, 2);
                double percentageCompletedPlans = Math.Round(((double)totalCompletedPlans / totalActionPlans) * 100, 2);

                var donutChartArray = new[] { percentageCompletedPlans, percentagePendingPlans };


                cache.Set("actionPlanDonutChart", donutChartArray, TimeSpan.FromSeconds(30));

                return donutChartArray;
            }

        }

        public async Task<object> GetActionPlanChartBarChart()
        {  

            if (cache.TryGetValue("actionPlanBarChart", out List<object> cachedData))
            {
                return cachedData;
            }

            using (var db = new IRSDbContext())
            {
                var aggregatedReport = await (from ap in db.ActionPlans
                                              join mt in db.MaintenanceTeams
                                                  on ap.MaintenanceStaffId equals mt.Id
                                              group ap by mt.Name into g
                                              select new
                                              {
                                                  count = g.Count(),
                                                  team = g.Key
                                              }).ToListAsync();

                var barChartObj = new
                {
                    xLabel = aggregatedReport.Select(r => r.count).ToArray(),
                    yLabel = aggregatedReport.Select(r => r.team).ToArray()
                };

                cache.Set("actionPlanBarChart", barChartObj, TimeSpan.FromSeconds(30));

                return barChartObj;
            }


        }

        public async Task<object> GetActionPlanChartLineChart()
        {

            if (cache.TryGetValue("actionPlanLineChart", out List<object> cachedData))
            {
                return cachedData;
            }

            using (var db = new IRSDbContext())
            {
                var completedOverTime = await db.ActionPlans
                    .Where(a => a.CompletedDate != null && a.Status == 30)
                    .GroupBy(a => a.CompletedDate.Value.Date)
                    .Select(g => new { date = g.Key, completed = g.Count() })
                    .OrderBy(m => m.date)
                    .ToListAsync();

                var pendingOverTime = await db.ActionPlans
                    .Where(a => a.CompletedDate == null && a.Status != 30)
                    .GroupBy(a => a.DueDate.Date)
                    .Select(g => new { date = g.Key, pending = g.Count() })
                    .OrderBy(m => m.date)
                    .ToListAsync();

                var lineChartObj = new
                {
                    completedOverTime,
                    pendingOverTime
                };

                cache.Set("actionPlanLineChart", lineChartObj, TimeSpan.FromSeconds(30));

                return lineChartObj;
            }


        }





    }
}
