using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Monitoring.ServiceEndpoints;

public interface IServiceEndpointAppService : ICrudAppService<
    ServiceEndpointDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateServiceEndpointDto,
    CreateUpdateServiceEndpointDto>
{
}
