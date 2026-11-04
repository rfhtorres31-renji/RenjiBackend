using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using renjibackend.Data;
using renjibackend.DTO;
using renjibackend.Interfaces;


namespace renjibackend.Services
{  

    public class Caching 
    {

        private readonly ICacheRepository _cacheRepo;
        private readonly IMemoryCache _cache;

        public Caching(IMemoryCache cache, ICacheRepository cacheRepo)
        {
            this._cacheRepo = cacheRepo;
            this._cache = cache;
        }

        // Controller for KPI Computations and Incident Reports List
        public async Task<object> GetIncidentReportsListCaching()
        {

             if (_cache.TryGetValue("reports", out List<object> cachedData))
             {
                 return cachedData;
             }
           
             var reports = await _cacheRepo.GetIncidentReportsList();

             var reportCountLists = await _cacheRepo.GetReportCounts();

             int noOfOpenReports = reportCountLists.Where(u => u.Status == 10).Count(); 
             int noOfInProgressReports = reportCountLists.Where(u => u.Status == 20).Count();
             int noOfResolvedReports = reportCountLists.Where(u => u.Status == 30).Count();

             var reportCounts = new { open = noOfOpenReports, inProgress = noOfInProgressReports, resolved = noOfResolvedReports };

             var reportsObj = new { data = reports, reportCounts };

             _cache.Set("reports", reportsObj, TimeSpan.FromSeconds(30));

             return reportsObj;
 
        }


        public async Task<object> GetTopIncidentReportsByUsers()
        {
            if (_cache.TryGetValue("topIncidentReportsByUsers", out List<object> cachedData))
            {
                return cachedData;
            }

            var query = await _cacheRepo.GetTopIncidentReports();

            _cache.Set("topIncidentReportsByUsers", query, TimeSpan.FromSeconds(30));
            return query;
            
        }


        // API Function Controller for Getting Top Users by how many accident they report
        public async Task<object> GetTopUsersbyReports()
        {

            if (_cache.TryGetValue("getTopUsersbyReports", out List<object> cachedData))
            {
                return cachedData;
            }


            var query = await _cacheRepo.GetTopUsersbyReports();

            _cache.Set("getTopUsersbyReports", query, TimeSpan.FromSeconds(30));

            return query;
        }


        public async Task<object> GetSummaryReportsBarChart_1Caching()
        {

            if (_cache.TryGetValue("barChart", out List<BarChart1Dto> cachedData))
            {
                return cachedData;
            }


             var result1 = await _cacheRepo.GetSummaryReportsBarChart_1Caching();
           
             _cache.Set("barChart", result1, TimeSpan.FromSeconds(30));
             return result1;
          
        }



        public async Task<object> GetSummaryReportsPieChart_2Caching()
        {

            if (_cache.TryGetValue("pieChart", out List<PieChartDto> cachedData))
            {
                return cachedData;
            }

            var query = await _cacheRepo.GetSummaryReportsPieChart_2Caching();

            _cache.Set("pieChart", query, TimeSpan.FromSeconds(30));

            return query;     
        }



        public async Task<object> GetActionPlanChartDonutChart()
        {

            if (_cache.TryGetValue("actionPlanDonutChart", out List<object> cachedData))
            {
                return cachedData;
            }

            var donutChartArray = await _cacheRepo.GetActionPlanChartDonutChart();

             _cache.Set("actionPlanDonutChart", donutChartArray, TimeSpan.FromSeconds(30));

            return donutChartArray; 

        }



        public async Task<object> GetActionPlanChartBarChart()
        {  

            if (_cache.TryGetValue("actionPlanBarChart", out List<object> cachedData))
            {
                return cachedData;
            }


            var barChartObj = await _cacheRepo.GetActionPlanChartBarChart();

            _cache.Set("actionPlanBarChart", barChartObj, TimeSpan.FromSeconds(30));

            return barChartObj;
        }



        public async Task<object> GetActionPlanChartLineChart()
        {

            if (_cache.TryGetValue("actionPlanLineChart", out List<object> cachedData))
            {
                return cachedData;
            }

            var lineChartObj = await _cacheRepo.GetActionPlanChartLineChart();

            _cache.Set("actionPlanLineChart", lineChartObj, TimeSpan.FromSeconds(30));

            return lineChartObj;
          
        }





    }
}
