using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface ICategoryService
{
    void Add(Category category);
    
    void Update(Category category);
    
    void Delete(Category category);
    
    void Delete(Guid id);
    
    List<Category> GetAll();
    
    Category Get(Guid id);
}   

internal class CategoryService(DataDbContext dataBase, CommissionUtilityService commissionUtilityService) : ICategoryService
{
    private const string AllKey = "AllCategoriesTree";
    
    internal void Add(Category category)
    {
        dataBase.Categories.Add(category);
        Cache.Remove(AllKey);
    }

    void ICategoryService.Add(Category category)
    {
        Add(category);
        dataBase.SaveChanges();
    }

    internal void Update(Category category)
    {
        var existing = dataBase.Categories.FirstOrDefault(c => c.Id == category.Id);
        if (existing == null)
        {
            return;
        }
        existing.CopyPossibleProperties(category);
        dataBase.Categories.Update(existing);
        Cache.Remove(AllKey);
    }

    void ICategoryService.Update(Category category)
    {
        Update(category);
        dataBase.SaveChanges();
    }

    internal void Delete(Category category)
    {
        foreach (var child in dataBase.Categories.Where(c => c.ParentId == category.Id).ToList())
        {
            Delete(child);
        }
        commissionUtilityService.Delete(category);
        dataBase.Categories.Remove(category);
        Cache.Remove(AllKey);
    }

    void ICategoryService.Delete(Category category)
    {
        Delete(category);
        dataBase.SaveChanges();
    }

    internal void Delete(Guid id)
    {
        var category = dataBase.Categories.FirstOrDefault(p => p.Id == id);
        if (category != null)
        {
            Delete(category);
        }
    }

    void ICategoryService.Delete(Guid id)
    {
        Delete(id);
        dataBase.SaveChanges();
    }
    internal List<Category> GetAll() => Cache.GetOrCreate(AllKey, 
        () => dataBase.Categories.Include(c => c.Children).Where(c => c.ParentId == null).ToList());

    List<Category> ICategoryService.GetAll() => GetAll();

    internal Category Get(Guid id) => dataBase.Categories.FirstOrDefault(c => c.Id == id);

    Category ICategoryService.Get(Guid id) => Get(id);
}