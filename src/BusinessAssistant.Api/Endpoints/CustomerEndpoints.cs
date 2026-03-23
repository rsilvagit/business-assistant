using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Exceptions;
using BusinessAssistant.Api.Services;
using FluentValidation;

namespace BusinessAssistant.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/customers")
            .WithTags("Customers")
            .RequireAuthorization();

        group.MapGet("/", async (ICustomerService service) =>
        {
            var customers = await service.GetAllAsync();
            return Results.Ok(customers);
        })
        .Produces<IEnumerable<CustomerResponse>>()
        .WithName("GetAllCustomers")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, ICustomerService service) =>
        {
            var customer = await service.GetByIdAsync(id);
            return Results.Ok(customer);
        })
        .Produces<CustomerResponse>()
        .WithName("GetCustomerById")
        .WithOpenApi();

        group.MapPost("/", async (
            CreateCustomerRequest request,
            IValidator<CreateCustomerRequest> validator,
            ICustomerService service) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                throw BadRequest400Exception.ValidationResult(validation.Errors);

            var customer = await service.CreateAsync(request);
            return Results.Created($"/api/v1/customers/{customer.Id}", customer);
        })
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .WithName("CreateCustomer")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCustomerRequest request,
            IValidator<UpdateCustomerRequest> validator,
            ICustomerService service) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                throw BadRequest400Exception.ValidationResult(validation.Errors);

            var customer = await service.UpdateAsync(id, request);
            return Results.Ok(customer);
        })
        .Produces<CustomerResponse>()
        .WithName("UpdateCustomer")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ICustomerService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .WithName("DeleteCustomer")
        .WithOpenApi();
    }
}
