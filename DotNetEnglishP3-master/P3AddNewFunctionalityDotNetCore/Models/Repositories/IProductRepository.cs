using P3AddNewFunctionalityDotNetCore.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P3AddNewFunctionalityDotNetCore.Models.Repositories
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAllProducts();
        void UpdateProductStocks(int productId, int quantityToRemove);
        Task SaveProduct(Product product);
        void DeleteProduct(int id);
        Task<Product> FindProductByNameAsync(string name);
        Task<Product> UpdateProduct(Product product);
        Task<Product> GetProduct(int id);
        Task<IList<Product>> GetProduct();
    }
}
