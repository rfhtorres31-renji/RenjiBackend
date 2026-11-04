using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using renjibackend.Data;
using renjibackend.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace renjibackend.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> EmailExistAsync(string email);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<User> GetUserByEmailAsync(string email);
    }

    public interface ICacheRepository
    {
        Task<object> GetIncidentReportsList();
        Task<List<IncidentReport>> GetReportCounts();
        Task<object> GetTopIncidentReports();
        Task<object> GetTopUsersbyReports();
        Task<object> GetSummaryReportsBarChart_1Caching();
        Task<object> GetSummaryReportsPieChart_2Caching();
        Task<object> GetActionPlanChartDonutChart();
        Task<object> GetActionPlanChartBarChart();
        Task<object> GetActionPlanChartLineChart();
    }

}

