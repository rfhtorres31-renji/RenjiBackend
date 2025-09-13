using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using renjibackend.Data;
using renjibackend.DTO;
using renjibackend.Models;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using renjibackend.Services;

namespace renjibackend.APIControllers
{
    [ApiController]
    [Route("api/actionplan")]
    public class ActionPlanController : ControllerBase
    {
        private readonly IRSDbContext db;
        private Response response = new Response();
        private readonly Caching cache;

        public ActionPlanController(IRSDbContext _db, Caching _cache)
        {
            this.db = _db;
            this.cache = _cache;
        }


        [HttpPost("post")]
        public async Task<IActionResult> PostActionPlan([FromBody] NewActionPlan.ActionPlanDto actionPlan)
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

                var newActionPlan = new ActionPlan
                {
                    IncidentReportId = actionPlan.IncidentReportID,
                    ActionDetail = actionPlan.Form.ActionDescription,
                    MaintenanceStaffId = actionPlan.Form.PersonInCharge,
                    DueDate = actionPlan.Form.TargetDate,
                    ActionType = actionPlan.Form.ActionTypes,
                    Priority = actionPlan.Form.Priority,
                    Status = 10, // 10 - In Progress, 20 - Finished
                };

                db.ActionPlans.Add(newActionPlan);
                await db.SaveChangesAsync();

                var incidentReportRecord = await db.IncidentReports.Where(u => u.Id == actionPlan.IncidentReportID).FirstOrDefaultAsync();

                if (incidentReportRecord != null)
                {
                    incidentReportRecord.Status = 20; // Change status to In Progress
                    incidentReportRecord.ActionPlanId = newActionPlan.Id;
                    await db.SaveChangesAsync();
                }

                response.success = true;
                response.message = "Action Plan Added Successfully";

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
        public async Task<IActionResult> GetActionPlan()
        {
            var query = await cache.GetActionPlanCaching();

            response.success = true;
            response.message = "Successfully Retrieved Records";
            response.details = new { data = query };

            return Ok(response);
        }


        [HttpPut("put")]
        [Authorize]
        public async Task<IActionResult> PutActionPlan(int actionID, int incidentReportID, [FromBody] UpdateActionPlanDto updateData)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            Debug.WriteLine(incidentReportID);
            Debug.WriteLine($"Action ID: {actionID}, Incident Report ID: ${incidentReportID}");

            try
            {
                var actionPlan = await db.ActionPlans.Where(u => u.Id == actionID).FirstOrDefaultAsync();

                if (actionPlan == null)
                {
                    response.success = false;
                    response.message = "No Action Plan";
                    return BadRequest(response);
                }

                if (updateData.NewActionPlanStatus == 30)
                {
                    Debug.WriteLine("Executed");
                    var incidentReport = await db.IncidentReports.Where(u => u.Id == incidentReportID).FirstOrDefaultAsync();

                    if (incidentReport == null)
                    {
                        response.success = false;
                        response.message = "No Incident Report";
                        return BadRequest(response);
                    }

                    incidentReport.Status = 30;

                }

                actionPlan.Status = updateData.NewActionPlanStatus;
                actionPlan.Remarks = updateData.Remarks;
                actionPlan.CompletedDate = DateTime.UtcNow;

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                response.success = true;
                response.message = "Ok";
                return Ok(response);
            }

            catch (Exception err)
            {
                await transaction.RollbackAsync();

                response.success = false;
                response.message = "An error occured during updating of Action Plan";
                response.details = err.Message;
                return BadRequest(response);
            }

        }

        [HttpGet("get/kpi")]
        [Authorize]
        public async Task<IActionResult> GetActionPlanKPI()
        {
            // KPI for Average Days Overdue
            var today = DateTime.UtcNow.Date;

            var overDueActionPlans = await db.IncidentReports
                                      .Include(i => i.ActionPlan)
                                      .Include(i => i.Department)
                                      .Include(i => i.Accident)
                                      .Where(u => u.ActionPlanId != null && u.ActionPlan.DueDate < today).ToListAsync();


            int totalOverdueActions = db.IncidentReports
                                      .Include(i => i.ActionPlan)
                                      .Include(i => i.Department)
                                      .Include(i => i.Accident)
                                      .Where(u => u.ActionPlanId != null && u.ActionPlan.DueDate < today).Count();

            double count = 0;

            foreach (var plan in overDueActionPlans)
            {
                if (plan.ActionPlan != null)
                {

                    var dueDate = plan.ActionPlan.DueDate;
                    double dueDays = (today - dueDate).TotalDays;

                    count = count + dueDays;
                }
            }

            double averageDueDays = totalOverdueActions != 0 ? count / totalOverdueActions : 0.0;
            averageDueDays = Math.Round(averageDueDays, 2);

            Debug.WriteLine($"Total Over Due Plans: {totalOverdueActions}");
            Debug.WriteLine($"Average Due Days: {averageDueDays}");


            // % Completed on Time
            int totalCompletedPlansOnStatus = db.IncidentReports
                                          .Include(i => i.ActionPlan)
                                          .Include(i => i.Department)
                                          .Include(i => i.Accident)
                                          .Where(u => u.ActionPlanId != null && u.ActionPlan.Status == 30).Count();

            int totalCompletedPlansBeforeDueDate = db.IncidentReports
                                          .Include(i => i.ActionPlan)
                                          .Include(i => i.Department)
                                          .Include(i => i.Accident)
                                          .Where(u => u.ActionPlanId != null &&
                                                 u.ActionPlan.Status == 30 &&
                                                 u.ActionPlan.CompletedDate <= u.ActionPlan.DueDate).Count();

            double percentageOnTime = (totalCompletedPlansBeforeDueDate / totalCompletedPlansOnStatus) * 100;
            Debug.WriteLine($"Percentage On Time: {percentageOnTime}");


            // Total Action Plans that are in In Progress 
            int noOfActivePlans = db.IncidentReports
                              .Include(i => i.ActionPlan)
                              .Include(i => i.Department)
                              .Include(i => i.Accident)
                              .Where(u => u.ActionPlanId != null && u.ActionPlan.Status != 30).Count();

            Debug.WriteLine(noOfActivePlans);

            response.success = true;
            response.message = "Ok";
            response.details = new { averageDueDays = averageDueDays, percentageOnTime = percentageOnTime, noOfActivePlans = noOfActivePlans };

            return Ok(response);
        }


        [HttpGet("get/chart")]
        [Authorize]
        public async Task<IActionResult> GetActionPlanChart()
        {

            object[] result = await Task.WhenAll(cache.GetActionPlanChartDonutChart(), cache.GetActionPlanChartBarChart(), cache.GetActionPlanChartLineChart());

            var donutChartObj = result[0];
            var barChartObj = result[1];
            var lineChartObj = result[2];

            response.success = true;
            response.message = "Ok";
            response.details = new { donutChart = donutChartObj, barChart = barChartObj, lineChart = lineChartObj };

            return Ok(response);
        }



    }
}
