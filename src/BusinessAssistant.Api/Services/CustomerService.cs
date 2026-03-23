using BusinessAssistant.Api.Data;
using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessAssistant.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Document = request.Document,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse?> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer is null ? null : MapToResponse(customer);
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync()
    {
        var customers = await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return customers.Select(MapToResponse);
    }

    public async Task<CustomerResponse?> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return null;

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Document = request.Document;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return false;

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private static CustomerResponse MapToResponse(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, customer.Phone,
            customer.Document, customer.IsActive, customer.CreatedAt);
}
