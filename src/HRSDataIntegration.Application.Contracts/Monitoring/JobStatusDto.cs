using System;

namespace HRSDataIntegration.Monitoring;

public class JobStatusDto
{
    public Guid Id { get; set; }

    public int Code { get; set; }

    public string Title { get; set; }

    public string JobGroupTitle { get; set; }

    public string JobRastehTitle { get; set; }

    public bool JobIsActive { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime LastActivityTime { get; set; }

    public string StateTitle { get; set; }

    public string LastActionTitle { get; set; }
}
