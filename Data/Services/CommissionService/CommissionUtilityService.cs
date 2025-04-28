using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Model.Database;

namespace Data.Services;

internal class CommissionUtilityService(DataDbContext dataBase)
{
    internal IIncludableQueryable<CommissionCategory, ICollection<CommissionSize>> CommissionCategories => 
        dataBase.CommissionCategories.Include(cc => cc.CommissionSizes);
    
    internal void ClearSalePlatformCache(Guid salePlatformId) => Cache.Remove(CommissionUtility.CacheKeyForPlatform(salePlatformId));
    internal void ClearCategoryCache(Guid categoryId) => Cache.Remove(CommissionUtility.CacheKeyForCategory(categoryId));

    private void RemoveRange(List<CommissionCategory> categories)
    {
        dataBase.Commissions.RemoveRange(categories.SelectMany(c => c.CommissionSizes));
        dataBase.CommissionCategories.RemoveRange(categories);
        foreach (var id in categories.Select(c => c.SalePlatformId).Distinct())
        {
            ClearSalePlatformCache(id);
        }
    }

    internal void Delete(SalePlatform salePlatform) => 
        RemoveRange(CommissionCategories.Where(c => c.SalePlatformId == salePlatform.Id).ToList());

    internal void Delete(Category category) => 
        RemoveRange(CommissionCategories.Where(c => c.CategoryId == category.Id).ToList());
}