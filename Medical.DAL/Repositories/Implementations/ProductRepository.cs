using Medical.DAL.Context;
using Medical.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.DAL.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetAllNamesAsync()
    {
        return await _context.Products
            .Select(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByNameAsync(string productName)
    {
        return await _context.Products
            .Where(p => p.Name == productName)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetIdByNameAsync(string productName)
    {
        return await _context.Products
            .Where(p => p.Name == productName)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
    }
}