using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Services;
using FluentValidation;

namespace BusinessAssistant.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
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
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        })
        .Produces<CustomerResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetCustomerById")
        .WithOpenApi();

        group.MapPost("/", async (
            CreateCustomerRequest request,
            IValidator<CreateCustomerRequest> validator,
            ICustomerService service) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var customer = await service.CreateAsync(request);
            return Results.Created($"/api/customers/{customer.Id}", customer);
        })
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
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
                return Results.ValidationProblem(validation.ToDictionary());

            var customer = await service.UpdateAsync(id, request);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        })
        .Produces<CustomerResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .WithName("UpdateCustomer")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ICustomerService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("DeleteCustomer")
        .WithOpenApi();
    }
}
