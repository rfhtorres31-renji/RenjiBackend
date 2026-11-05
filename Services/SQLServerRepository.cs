using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using renjibackend.Data;
using renjibackend.Interfaces;
using renjibackend.Models;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using renjibackend.DTO;

// Created using Single-Responsibility Principle
// This focuses only on communicating on SQL Server 

namespace renjibackend.Services
{
    public class SQLServerUserRepository : IAuthRepository, ICacheRepository, ITransaction
    {

        private readonly IRSDbContext _db;


        public SQLServerUserRepository(IRSDbContext db)
        {
            _db = db;
        }


        public async Task<bool> EmailExistAsync(string email, CancellationToken cancellationToken)
        {
            return await _db.Users.AsNoTracking().AnyAsync(u=>u.Email == email, cancellationToken);
        }


        public async Task AddUserAsync(User user, CancellationToken cancellationToken)
        {
            await _db.Users.AddAsync(user, cancellationToken);
        }


        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
             await _db.Database.BeginTransactionAsync(cancellationToken);
        }
        
        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
             await _db.Database.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
             await _db.Database.RollbackTransactionAsync(cancellationToken);
        }

        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _db.Users.AsNoTracking().Where(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<object> GetIncidentReportsList()
        {

            using (var dbContext = new IRSDbContext())
            {
                return await dbContext.IncidentReports.Select(n => new {
                    ID = n.Id,
                    Type = dbContext.Accidents.Where(u => u.Id == n.AccidentId).Select(n => n.Name).FirstOrDefault(),
                    Description = n.Description,
                    Location = n.Location,
                    ReportedDate = n.ReportedDate,
                    ReportedBy = dbContext.Users.Where(u => u.Id == n.ReportedBy).Select(n => n.FirstName + ' ' + n.LastName + ',' + ' ' +
                                 dbContext.Departments.Where(u => u.Id == n.DepartmentId).Select(n => n.Name).FirstOrDefault()
                                                ).FirstOrDefault(),
                    Status = n.Status == 10 ? "Open" :
                                                 n.Status == 20 ? "In Progress" :
                                                 n.Status == 30 ? "Resolved" : ""
                }).AsNoTracking().ToListAsync();
            }
                
        }




        public async Task<List<IncidentReport>> GetReportCounts()
        {
            return await _db.IncidentReports.ToListAsync();
        }



        public async Task<object> GetTopIncidentReports()
        {
            using (var dbContext = new IRSDbContext())
            {
                return await (from ir in dbContext.IncidentReports
                              join u in dbContext.Users on ir.ReportedBy equals u.Id
                              group ir by u.Id into g
                              select new
                              {
                                  Name = dbContext.Users.Where(u => u.Id == g.Key).Select(n =>
                                     n.FirstName + " " + n.LastName + ", " + dbContext.Departments.Where(u => u.Id == n.DepartmentId).Select(n => n.Name).FirstOrDefault()

                                  ).FirstOrDefault(),
                                  Count = g.Count()
                              }
                              ).OrderByDescending(o => o.Count).AsNoTracking().ToListAsync();
            }
                
        }



        public async Task<object> GetTopUsersbyReports()
        {
            DateTime now = DateTime.Now;

            int month = now.Month;
            int year = now.Year;

            using (IRSDbContext dbContext = new IRSDbContext())
            {
                return await (from ir in dbContext.IncidentReports
                                    join u in dbContext.Users on ir.ReportedBy equals u.Id
                                    where ir.ReportedDate.Month == month && ir.ReportedDate.Year == year
                                    group ir by u.Id into g
                                    select new
                                    {
                                        Name = dbContext.Users.Where(u => u.Id == g.Key).Select(n =>
                                           n.FirstName + " " + n.LastName + ", " + dbContext.Departments.Where(u => u.Id == n.DepartmentId).Select(n => n.Name).FirstOrDefault()
                                         ).FirstOrDefault(),
                                        Count = g.Count()
                                    }
                                  ).OrderByDescending(o => o.Count).AsNoTracking().ToListAsync();

            }

        }


        public async Task<object> GetSummaryReportsBarChart_1Caching()
        {
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

                var result1 = await query1.OrderByDescending(x => x.y).AsNoTracking().ToListAsync();

                return result1;
            }
        }



        public async Task<object> GetSummaryReportsPieChart_2Caching()
        {
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
                             }).AsNoTracking().ToListAsync();

                   return query;
             }
        }

        public async Task<object> GetActionPlanChartDonutChart()
        {
            using (var db = new IRSDbContext())
            {
                var pendingActionPlans = await db.IncidentReports
                                         .Include(i => i.ActionPlan)
                                         .Include(i => i.Department)
                                         .Include(i => i.Accident)
                                        .Where(u => u.ActionPlan.Status != 30).AsNoTracking().ToListAsync();

                int totalPendingPlans = pendingActionPlans.Count();

                var completedActionPlans = await db.IncidentReports
                                              .Include(i => i.ActionPlan)
                                              .Include(i => i.Department)
                                              .Include(i => i.Accident)
                                              .Where(u => u.ActionPlan.Status == 30).AsNoTracking().ToListAsync();


                int totalCompletedPlans = completedActionPlans.Count();


                int totalActionPlans = db.IncidentReports
                                      .Include(i => i.ActionPlan)
                                      .Include(i => i.Department)
                                      .Include(i => i.Accident).AsNoTracking().Count();



                double percentagePendingPlans = Math.Round(((double)totalPendingPlans / totalActionPlans) * 100, 2);
                double percentageCompletedPlans = Math.Round(((double)totalCompletedPlans / totalActionPlans) * 100, 2);

                var donutChartArray = new[] { percentageCompletedPlans, percentagePendingPlans };

                return donutChartArray;
            }
        }

        public async Task<object> GetActionPlanChartBarChart()
        {
            using (var db = new IRSDbContext())
            {
                var aggregatedReport = await (from ap in db.ActionPlans
                                              join mt in db.MaintenanceTeams
                                                  on ap.MaintenanceStaffId equals mt.Id
                                              where ap.Status == 30
                                              group ap by mt.Name into g
                                              select new
                                              {
                                                  count = g.Count(),
                                                  team = g.Key
                                              }).AsNoTracking().ToListAsync();

                var barChartObj = new
                {
                    xLabel = aggregatedReport.Select(r => r.count).ToArray(),
                    yLabel = aggregatedReport.Select(r => r.team).ToArray()
                };

                return barChartObj;
            }
        }

        public async Task<object> GetActionPlanChartLineChart()
        {
               using (var db = new IRSDbContext())
               {
                   var completedOverTime = await db.ActionPlans
                        .Where(a => a.CompletedDate != null && a.Status == 30)
                        .GroupBy(a => a.CompletedDate.Value.Date)
                        .Select(g => new { date = g.Key, completed = g.Count() })
                        .OrderBy(m => m.date)
                        .AsNoTracking()
                        .ToListAsync();

                   var pendingOverTime = await db.ActionPlans
                        .Where(a => a.CompletedDate == null && a.Status != 30)
                        .GroupBy(a => a.DueDate.Date)
                        .Select(g => new { date = g.Key, pending = g.Count() })
                        .OrderBy(m => m.date)
                        .AsNoTracking()
                        .ToListAsync();

                    var lineChartObj = new
                    {
                        completedOverTime,
                        pendingOverTime
                    };

                    return lineChartObj;
               }
        }


    }
}
