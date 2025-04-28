using Microsoft.EntityFrameworkCore.Query;
using Model.Database;
using Model.Extentions;
using Model.Requests;

namespace Data.Services;

public interface ICommissionService
{
    void Add(CommissionCategory commissionCategory);

    void Update(CommissionCategory commissionCategory);

    void Delete(CommissionCategory commissionCategory);

    void Delete(Guid id);

    CommissionRequest Get(Guid id);

    List<CommissionRequest> GetCategory(Guid categoryId);
}

internal partial class CommissionService(
    DataDbContext dataBase,
    CategoryService categoryService,
    SalePlatformService salePlatformService,
    CommissionUtilityService utilities) : ICommissionService
{
    private List<SalePlatform> SalePlatforms => salePlatformService.GetAll();

    private IIncludableQueryable<CommissionCategory, ICollection<CommissionSize>> CommissionCategories =>
        utilities.CommissionCategories;

    private static string GetKey(Guid salePlatformId, Guid categoryId) => $"Commission_{salePlatformId}_{categoryId}";
    private static string GetKey(Guid id) => $"Commission_{id}";
    private static string GetKey(CommissionCategory category) => GetKey(category.SalePlatformId, category.CategoryId);

    internal void Add(CommissionCategory commissionCategory)
    {
        dataBase.CommissionCategories.Add(commissionCategory);
        if (commissionCategory.CommissionSizes == null)
        {
            return;
        }

        dataBase.SaveChanges();
        foreach (var size in commissionCategory.CommissionSizes)
        {
            size.CommissionCategoryId = commissionCategory.Id;
        }
    }

    void ICommissionService.Add(CommissionCategory commissionCategory)
    {
        Add(commissionCategory);
        dataBase.SaveChanges();
        Cache.Set(GetKey(commissionCategory), commissionCategory);
        ClearCache(commissionCategory);
    }

    internal void Update(CommissionCategory commissionCategory)
    {
        var existing = CommissionCategories
            .FirstOrDefault(c => c.Id == commissionCategory.Id);
        if (existing == null)
        {
            return;
        }

        existing.CopyPossibleProperties(commissionCategory);
        dataBase.CommissionCategories.Update(existing);

        foreach (var size in commissionCategory.CommissionSizes)
        {
            size.CommissionCategoryId = commissionCategory.Id;
        }

        UpdateCommissionSizes(existing, commissionCategory.CommissionSizes);
    }

    private void UpdateCommissionSizes(CommissionCategory existingCategory, ICollection<CommissionSize> updatedSizes)
    {
        if (updatedSizes == null)
        {
            return;
        }

        if (existingCategory != null)
        {
            dataBase.Commissions.RemoveRange(existingCategory.CommissionSizes
                .Where(existingSize => updatedSizes.All(updatedSize => updatedSize.Id != existingSize.Id)));
        }

        foreach (var updatedSize in updatedSizes)
        {
            var existingSize = existingCategory?.CommissionSizes?.FirstOrDefault(s => s.Id == updatedSize.Id);
            if (existingSize == null)
            {
                dataBase.Commissions.Add(updatedSize);
                continue;
            }

            existingSize.CopyPossibleProperties(updatedSize);
            dataBase.Commissions.Update(existingSize);
        }
    }

    void ICommissionService.Update(CommissionCategory commissionCategory)
    {
        Update(commissionCategory);
        dataBase.SaveChanges();
        Cache.Set(GetKey(commissionCategory), commissionCategory);
        ClearCache(commissionCategory);
    }

    internal void Delete(CommissionCategory commissionCategory)
    {
        if (commissionCategory.CommissionSizes != null)
        {
            dataBase.Commissions.RemoveRange(commissionCategory.CommissionSizes);
        }

        dataBase.CommissionCategories.Remove(commissionCategory);
        Cache.Remove(GetKey(commissionCategory));
        ClearCache(commissionCategory);
    }

    void ICommissionService.Delete(CommissionCategory commissionCategory)
    {
        Delete(commissionCategory);
        dataBase.SaveChanges();
    }

    private void ClearCache(CommissionCategory commissionCategory)
    {
        utilities.ClearSalePlatformCache(commissionCategory.SalePlatformId);
        utilities.ClearCategoryCache(commissionCategory.CategoryId);
    }

    internal void Delete(Guid id)
    {
        var commissionCategory = Get(id);
        if (commissionCategory != null)
        {
            Delete(commissionCategory);
        }
    }

    void ICommissionService.Delete(Guid id)
    {
        Delete(id);
        dataBase.SaveChanges();
    }

    internal CommissionCategory Get(Guid id) => Cache.GetOrCreate(GetKey(id),
        () => CommissionCategories.FirstOrDefault(c => c.Id == id));

    CommissionRequest ICommissionService.Get(Guid id)
    {
        var commissionCategory = Get(id);
        var inheritedCommissions = GetInheritedCommissions(commissionCategory);
        return new CommissionRequest(commissionCategory, inheritedCommissions);
    }

    internal CommissionCategory Get(Guid salePlatformId, Guid categoryId) => Cache.GetOrCreate(
        GetKey(salePlatformId, categoryId),
        () => CommissionCategories.FirstOrDefault(c => c.CategoryId == categoryId && c.SalePlatformId == salePlatformId)
    );

    internal List<CommissionCategory> GetCategory(Guid categoryId) => Cache.GetOrCreate(
        CommissionUtility.CacheKeyForCategory(categoryId),
        () => CommissionCategories.Where(c => c.CategoryId == categoryId).ToList());

    List<CommissionRequest> ICommissionService.GetCategory(Guid categoryId)
    {
        var categories = GetCategory(categoryId);
        var inheritedCommissions = GetInheritedCommissionsForCategory(categoryId);
        return SalePlatforms.Select(s => new CommissionRequest(
            categories.FirstOrDefault(c => c.SalePlatformId == s.Id) ?? new CommissionCategory
            {
                SalePlatformId = s.Id,
                CategoryId = categoryId,
            },
            inheritedCommissions.GetValueOrDefault(s.Id)
        )).ToList();
    }
}