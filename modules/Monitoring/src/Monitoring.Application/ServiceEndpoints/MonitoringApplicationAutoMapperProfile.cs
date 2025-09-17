using AutoMapper;

namespace Monitoring.ServiceEndpoints;

public class MonitoringApplicationAutoMapperProfile : Profile
{
    public MonitoringApplicationAutoMapperProfile()
    {
        CreateMap<ServiceEndpoint, ServiceEndpointDto>()
            .ForMember(dest => dest.StatusSnapshots, opt => opt.MapFrom(src => src.StatusSnapshots));

        CreateMap<ServiceStatusSnapshot, ServiceStatusSnapshotDto>();

        CreateMap<ServiceStatusSnapshot, RunCheckResultDto>();
    }
}
