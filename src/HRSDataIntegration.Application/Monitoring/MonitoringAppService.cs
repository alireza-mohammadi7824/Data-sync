using System;
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

    // from codex/add-monitoringapplication-to-datasync-gi4p0i
    private readonly ISqlRepository<JobGroup> _jobGroupRepository;
    private readonly ISqlRepository<JobRasteh> _jobRastehRepository;

    // from main
    private readonly ISqlRepository<Unit> _unitRepository;
    private readonly ISqlRepository<Post> _postRepository;

    public MonitoringAppService(
        ISqlRepository<Job> jobRepository,
        ISqlRepository<JobDetail> jobDetailRepository,
        ISqlRepository<JobGroup> jobGroupRepository,
        ISqlRepository<JobRasteh> jobRastehRepository,
        ISqlRepository<Unit> unitRepository,
        ISqlRepository<Post> postRepository)
    {
        _jobRepository = jobRepository;
        _jobDetailRepository = jobDetailRepository;
        _jobGroupRepository = jobGroupRepository;
        _jobRastehRepository = jobRastehRepository;
        _unitRepository = unitRepository;
        _postRepository = postRepository;
    }

    public async Task<MonitoringDashboardDto> GetDashboardAsync()
    {
        // Queryables (با AsNoTracking برای کارایی بهتر dashboard)
        var jobQueryable = _jobRepository.GetQueryable().AsNoTracking();
        var jobDetailQueryable = _jobDetailRepository.GetQueryable().AsNoTracking();
        var jobGroupQueryable = _jobGroupRepository.GetQueryable().AsNoTracking();
        var jobRastehQueryable = _jobRastehRepository.GetQueryable().AsNoTracking();
        var unitQueryable = _unitRepository.GetQueryable().AsNoTracking();
        var postQueryable = _postRepository.GetQueryable().AsNoTracking();

        // Summary با ادغام هر دو سمت
        var summary = new MonitoringSummaryDto
        {
            TotalJobs = await AsyncExecuter.CountAsync(jobQueryable),
            ActiveJobs = await AsyncExecuter.CountAsync(jobQueryable.Where(job => job.IsActive)),
            TotalJobDetails = await AsyncExecuter.CountAsync(jobDetailQueryable),

            // from codex/add-monitoringapplication-to-datasync-gi4p0i
            TotalJobGroups = await AsyncExecuter.CountAsync(jobGroupQueryable),
            TotalJobRasteh = await AsyncExecuter.CountAsync(jobRastehQueryable),

            // from main
            TotalUnits = await AsyncExecuter.CountAsync(unitQueryable),
            TotalPosts = await AsyncExecuter.CountAsync(postQueryable)
        };

        // جزئیات آخرین 100 رکورد با include برای دسترسی به navigation ها
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

        // نرمالایز زمان‌ها و محاسبه‌ی GeneratedAt بر اساس آخرین فعالیت (ترجیح به منطق دقیق‌تر)
        foreach (var job in jobs)
        {
            job.LastActivityTime = Clock.Normalize(job.LastActivityTime);
        }

        var latestActivity = jobs.FirstOrDefault()?.LastActivityTime;
        summary.GeneratedAt = latestActivity.HasValue && latestActivity.Value != default
            ? Clock.Normalize(latestActivity.Value)
            : Clock.Normalize(Clock.Now);

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
