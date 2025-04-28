namespace Data.Services;

internal class CommissionUtility
{
    internal static string CacheKeyForPlatform(Guid salePlatformId) => $"CommissionsForPlatform_{salePlatformId}";
    internal static string CacheKeyForCategory(Guid categoryId) => $"CommissionCategories_{categoryId}";
}