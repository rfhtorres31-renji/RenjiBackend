using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using renjibackend.Data;
using renjibackend.DTO;
using renjibackend.Models;
using renjibackend.Services;
using renjibackend.Utility;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using static renjibackend.Services.Caching;



namespace renjibackend.APIControllers
{
    [ApiController]
    [Route("api/reports")]
    public class IncidentReportsController : ControllerBase
    {

        private readonly IRSDbContext db;
        private Response response = new Response();
        private readonly Caching cache;

        public IncidentReportsController(IRSDbContext _db, Caching _cache)
        {
            this.db = _db;
            this.cache = _cache;
        }


        [HttpPost("post")]
        [Authorize]
        public async Task<IActionResult> PostNewReport([FromBody] NewReport report)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    response.success = false;
                    response.message = "Model State is Invalid";
                    response.details = ModelState;
                    return BadRequest(response);
                }

                int userID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                string accidentTypeName = await db.Accidents.Where(u => u.Id == report.AccidentTypeId).Select(n => n.Name).FirstOrDefaultAsync() ?? "";
                int departmentId = await db.Users.Where(u => u.Id == userID).Select(n => n.DepartmentId).FirstOrDefaultAsync();

                Debug.WriteLine(accidentTypeName);

                var newReport = new IncidentReport
                {
                    Title = report.Title,
                    Description = report.Description,
                    Location = report.Location,
                    ReportedDate = DateTime.UtcNow,
                    ReportedBy = userID,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Status = 10,
                    AttachmentPath = "",
                    AccidentId = report.AccidentTypeId,
                    DepartmentId = departmentId,
                };


                db.IncidentReports.Add(newReport);
                await db.SaveChangesAsync();

                response.success = true;
                response.message = "New Reports Added Successfully";
                return Ok(response);

            }
            catch (Exception err)
            {
                response.success = false;
                response.message = "Internal Server Error";
                response.details = err.Message;

                return StatusCode(500, response);
            }
        }


        [HttpGet("get")]
        [Authorize]
        public async Task<IActionResult> GetReports(int userID)
        {
            // Runs three functions in parallel
            // This returns an object array with the awaited results
            object[] result = await Task.WhenAll(cache.GetIncidentReportsListCaching(), cache.GetSummaryReportsBarChart_1Caching(), cache.GetSummaryReportsPieChart_2Caching(), cache.GetTopIncidentReportsByUsers(), cache.GetTopUsersbyReports());

            var report =  result[0]; 
            var result1 = result[1]; 
            var result2 = result[2];
            var result3 = result[3];
            var result4 = result[4];

            response.success = true;
            response.message = "Success";
            response.details = new { incidentReports = report, barChart = result1, pieChart = result2, incidentReportsByUsers = result3, topUsersbyReports = result4};

            return Ok(response);
        }


        // Daily Incident Trends by Accident Type
        [HttpGet("summaryreports-2")]
        public IActionResult GetSummaryReports_2()
        {
            var queryResult = db.IncidentReports.AsNoTracking()
                .Join(db.Accidents.AsNoTracking(),
                      ir => ir.AccidentId,
                      a => a.Id,
                      (ir, a) => new { ir.ReportedDate, AccidentType = a.Name })
                .AsEnumerable() // move data to memory
                .GroupBy(x => new {
                    x.AccidentType,
                    ReportHour = new DateTime(x.ReportedDate.Year, x.ReportedDate.Month, x.ReportedDate.Day, x.ReportedDate.Hour, 0, 0)
                })
                .Select(g => new {
                    g.Key.AccidentType,
                    ReportHour = g.Key.ReportHour,
                    TotalIncidents = g.Count()
                })
                .OrderBy(x => x.ReportHour)
                .ThenBy(x => x.AccidentType)
                .ToList();


            var chartData = queryResult
                .GroupBy(x => x.AccidentType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new { x = x.ReportHour, y = x.TotalIncidents }).ToList()
                );

            response.success = true;
            response.message = "Success";
            response.details = new { data = chartData };

            return Ok(response);
        }


    }
}
