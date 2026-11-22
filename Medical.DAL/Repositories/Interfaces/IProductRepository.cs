using Medical.DAL.Entities;

namespace Medical.DAL.Repository;

public interface IProductRepository
{
    Task<List<string>> GetAllNamesAsync();
    Task<Product?> GetByNameAsync(string productName);
    Task<string?> GetIdByNameAsync(string productName);
}