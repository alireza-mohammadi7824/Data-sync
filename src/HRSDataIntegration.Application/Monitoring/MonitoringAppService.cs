using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HRSDataIntegration;
using HRSDataIntegration.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRSDataIntegration.Monitoring;

public class MonitoringAppService : HRSDataIntegrationAppService, IMonitoringAppService
{
    private readonly ISqlRepository<Job> _jobRepository;
    private readonly ISqlRepository<JobDetail> _jobDetailRepository;
    private readonly ISqlRepository<JobGroup> _jobGroupRepository;
    private readonly ISqlRepository<JobRasteh> _jobRastehRepository;

    public MonitoringAppService(
        ISqlRepository<Job> jobRepository,
        ISqlRepository<JobDetail> jobDetailRepository,
        ISqlRepository<JobGroup> jobGroupRepository,
        ISqlRepository<JobRasteh> jobRastehRepository)
    {
        _jobRepository = jobRepository;
        _jobDetailRepository = jobDetailRepository;
        _jobGroupRepository = jobGroupRepository;
        _jobRastehRepository = jobRastehRepository;
    }

    public async Task<MonitoringDashboardDto> GetDashboardAsync()
    {
        var jobQueryable = _jobRepository.GetQueryable();
        var jobDetailQueryable = _jobDetailRepository.GetQueryable();
        var jobGroupQueryable = _jobGroupRepository.GetQueryable();
        var jobRastehQueryable = _jobRastehRepository.GetQueryable();

        var summary = new MonitoringSummaryDto
        {
            TotalJobs = await AsyncExecuter.CountAsync(jobQueryable),
            ActiveJobs = await AsyncExecuter.CountAsync(jobQueryable.Where(job => job.IsActive)),
            TotalJobDetails = await AsyncExecuter.CountAsync(jobDetailQueryable),
            TotalJobGroups = await AsyncExecuter.CountAsync(jobGroupQueryable),
            TotalJobRasteh = await AsyncExecuter.CountAsync(jobRastehQueryable),
            GeneratedAt = Clock.Now
        };

        var jobDetails = await AsyncExecuter.ToListAsync(
            jobDetailQueryable
                .Include(detail => detail.Job)
                .Include(detail => detail.JobGroup)
                .Include(detail => detail.JobRasteh)
                .OrderByDescending(detail => detail.LastActivityTime)
                .Take(100)
        );

        var jobs = jobDetails
            .Select(detail => new JobStatusDto
            {
                Id = detail.Id,
                Code = detail.Code,
                Title = detail.Title,
                JobGroupTitle = detail.JobGroup?.Title,
                JobRastehTitle = detail.JobRasteh?.Title,
                JobIsActive = detail.Job?.IsActive ?? false,
                EffectiveFrom = ConvertOracleDate(detail.EffectiveDateFrom),
                EffectiveTo = ConvertOracleDate(detail.EffectiveDateTo),
                LastActivityTime = detail.LastActivityTime,
                StateTitle = detail.StateTitle,
                LastActionTitle = detail.LastActionTitle
            })
            .ToList();

        return new MonitoringDashboardDto
        {
            Summary = summary,
            Jobs = jobs
        };
    }

    private static DateTime? ConvertOracleDate(int value)
    {
        if (value <= 0)
        {
            return null;
        }

        var formatted = value.ToString("00000000", CultureInfo.InvariantCulture);
        if (DateTime.TryParseExact(formatted, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        return null;
    }
}
