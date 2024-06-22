using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface IProductService
{
    List<Product> GetAll(List<Guid> ids = null);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
    void Delete(Guid Id);
    Product Get(Guid id);
}

internal class ProductService(DataDbContext dataBase) : IProductService
{
    void IProductService.Add(Product product)
    {
        dataBase.Products.Add(product);
        dataBase.SaveChanges();
    }

    void IProductService.Update(Product product)
    {
        var existing = dataBase.Products.FirstOrDefault(c => c.Id == product.Id);
        if (existing == null)
        {
            return;
        }
        
        existing.CopyPossibleProperties(product);
        dataBase.Products.Update(existing);
        dataBase.SaveChanges();
    }

    void IProductService.Delete(Product product)
    {
        dataBase.Products.Remove(product);
        dataBase.SaveChanges();
    }

    void IProductService.Delete(Guid id)
    {
        var product = dataBase.Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            dataBase.Products.Remove(product);
            dataBase.SaveChanges();
        }
    }

    List<Product> IProductService.GetAll(List<Guid> ids) => ids == null 
        ? dataBase.Products.ToList()
        : dataBase.Products.Where(p => ids.Contains(p.Id)).ToList();

    Product IProductService.Get(Guid id) => dataBase.Products.FirstOrDefault(p => p.Id == id);
}