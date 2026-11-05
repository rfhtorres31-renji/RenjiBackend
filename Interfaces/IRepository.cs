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
    public interface ITransaction
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
   
    public interface IAuthRepository
    {
        Task<bool> EmailExistAsync(string email, CancellationToken cancellationToken);
        Task AddUserAsync(User user, CancellationToken cancellationToken);     
        Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
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

