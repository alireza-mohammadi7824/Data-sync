using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monitoring.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Monitoring.ServiceEndpoints;

public class ServiceEndpointAppService : CrudAppService<
    ServiceEndpoint,
    ServiceEndpointDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateServiceEndpointDto,
    CreateUpdateServiceEndpointDto>,
    IServiceEndpointAppService
{
    public ServiceEndpointAppService(IRepository<ServiceEndpoint, Guid> repository)
        : base(repository)
    {
        GetPolicyName = MonitoringPermissions.View;
        GetListPolicyName = MonitoringPermissions.Default;
        CreatePolicyName = MonitoringPermissions.Create;
        UpdatePolicyName = MonitoringPermissions.Edit;
        DeletePolicyName = MonitoringPermissions.Delete;
    }

    protected override Task<ServiceEndpoint> MapToEntityAsync(CreateUpdateServiceEndpointDto createInput)
    {
        var entity = new ServiceEndpoint(
            GuidGenerator.Create(),
            createInput.Name,
            createInput.ServiceType,
            createInput.Target,
            TimeSpan.FromSeconds(createInput.CheckIntervalSeconds),
            TimeSpan.FromSeconds(createInput.TimeoutSeconds),
            createInput.IsEnabled,
            createInput.Description);

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateServiceEndpointDto updateInput, ServiceEndpoint entity)
    {
        entity.Update(
            updateInput.Name,
            updateInput.ServiceType,
            updateInput.Target,
            TimeSpan.FromSeconds(updateInput.CheckIntervalSeconds),
            TimeSpan.FromSeconds(updateInput.TimeoutSeconds),
            updateInput.IsEnabled,
            updateInput.Description);

        return Task.CompletedTask;
    }

    protected override async Task<ServiceEndpoint> GetEntityByIdAsync(Guid id)
    {
        var queryable = await GetQueryableWithDetailsAsync();

        var entity = await queryable.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ServiceEndpoint), id);
        }

        return entity;
    }

    protected override Task<IQueryable<ServiceEndpoint>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return GetQueryableWithDetailsAsync();
    }

    protected override string GetDefaultSorting()
    {
        return nameof(ServiceEndpoint.Name);
    }

    protected virtual Task<IQueryable<ServiceEndpoint>> GetQueryableWithDetailsAsync()
    {
        return Repository.WithDetailsAsync(x => x.StatusSnapshots);
    }
}
