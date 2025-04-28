using Model.Database;

namespace Data.Services;

internal partial class CommissionService
{
    private List<Category> Categories => categoryService.GetAll();

    private static void FillCommissionsForChildren(
        Dictionary<Guid, List<CommissionSize>> dictionary,
        Category category,
        List<CommissionSize> parentCommissionSizes = null)
    {
        if (
            (!dictionary.TryGetValue(category.Id, out var commissionSizes) || commissionSizes == null)
            && category.ParentId != null
            && (parentCommissionSizes != null
                || dictionary.TryGetValue(category.ParentId.Value, out parentCommissionSizes) &&
                parentCommissionSizes != null
            ))
        {
            dictionary[category.Id] = commissionSizes = parentCommissionSizes;
        }

        if (category.Children?.Count > 0)
        {
            foreach (var child in category.Children)
            {
                FillCommissionsForChildren(dictionary, child, commissionSizes);
            }
        }
    }

    private Dictionary<Guid, List<CommissionSize>> GetCommissionsForSalePlatform(Guid salePlatformId)
    {
        return Cache.GetOrCreate(CommissionUtility.CacheKeyForPlatform(salePlatformId), () =>
        {
            var commissionCategoriesDictionary = CommissionCategories.Where(cc => cc.SalePlatformId == salePlatformId)
                .ToDictionary(cc => cc.CategoryId, cc => cc.CommissionSizes.ToList());
            foreach (var category in Categories)
            {
                FillCommissionsForChildren(commissionCategoriesDictionary, category);
            }

            return commissionCategoriesDictionary;
        });
    }

    internal Dictionary<Guid, List<CommissionSize>> GetCommissionsForSalePlatform(Guid salePlatformId,
        List<Guid> productIds)
    {
        var categoryDictionary = GetCommissionsForSalePlatform(salePlatformId);
        return dataBase.Products.Where(p => productIds.Contains(p.Id)).ToDictionary(
            p => p.Id,
            p => categoryDictionary.GetValueOrDefault(p.CategoryId));
    }

    /// <summary>
    /// Retrieves a dictionary of inherited commission sizes for a specific category across all available sale platforms.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category for which inherited commissions are retrieved.</param>
    /// <returns>A dictionary where the keys are sale platform IDs and the values are lists of commission sizes associated with the specified category.</returns>
    private Dictionary<Guid, List<CommissionSize>> GetInheritedCommissionsForCategory(Guid categoryId) =>
        SalePlatforms.Select(s => s.Id).Where(id => id != Guid.Empty).ToDictionary(
            id => id,
            id => GetCommissionsForSalePlatform(id).GetValueOrDefault(categoryId));

    private List<CommissionSize> GetInheritedCommissions(CommissionCategory commission) =>
        GetCommissionsForSalePlatform(commission.SalePlatformId).GetValueOrDefault(commission.CategoryId);

}