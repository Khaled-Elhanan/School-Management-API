using Application.Wrappers;
using MediatR;

namespace Application.Tenancy.Commands;

public class DeactivateTenantCommand : IRequest<IResponseWrapper>
{
    public required string TenantId { get; init; }
}

public class DeactivateTenantCommandHandler
    : IRequestHandler<DeactivateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService;

    public DeactivateTenantCommandHandler(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<IResponseWrapper> Handle(
        DeactivateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var deactivatedTenantId = await _tenantService
            .DeactivateTenantAsync(request.TenantId);

        return await ResponseWrapper<string>
            .SuccessAsync(
                data: deactivatedTenantId,
                message: "Tenant deactivated successfully"
            );
    }

}
