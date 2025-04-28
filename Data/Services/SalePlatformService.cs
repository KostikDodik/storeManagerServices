using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface ISalePlatformService
{
    List<SalePlatform> GetAll();
    SalePlatform Get(Guid id);
    void Add(SalePlatform salePlatform);
    void Update(SalePlatform salePlatform);
    void Delete(SalePlatform salePlatform);
    void Delete(Guid salePlatformId);
}

internal class SalePlatformService(DataDbContext dataBase, CommissionUtilityService commissionUtilityService) : ISalePlatformService
{
    private const string CacheKey = "SalePlatforms";
    void ISalePlatformService.Add(SalePlatform salePlatform)
    {
        dataBase.SalePlatforms.Add(salePlatform);
        dataBase.SaveChanges();
        Cache.Remove(CacheKey);
    }

    void ISalePlatformService.Update(SalePlatform salePlatform)
    {
        var existing = dataBase.SalePlatforms.FirstOrDefault(c => c.Id == salePlatform.Id);
        if (existing == null)
        {
            return;
        }

        existing.CopyPossibleProperties(salePlatform);
        dataBase.SalePlatforms.Update(existing);
        dataBase.SaveChanges();
    }

    private void Delete(SalePlatform salePlatform)
    {
        if (dataBase.Orders.Any(s => s.SalePlatformId == salePlatform.Id))
        {
            throw new InvalidOperationException("This Sale Platform already has active sales");
        }
        commissionUtilityService.Delete(salePlatform);
        commissionUtilityService.ClearSalePlatformCache(salePlatform.Id);
        dataBase.SalePlatforms.Remove(salePlatform);
    }

    void ISalePlatformService.Delete(SalePlatform salePlatform)
    {
        Delete(salePlatform);
        dataBase.SaveChanges();
    }

    void ISalePlatformService.Delete(Guid id)
    {
        var salePlatform = dataBase.SalePlatforms.FirstOrDefault(p => p.Id == id);
        if (salePlatform == null) return;
        Delete(salePlatform);
        dataBase.SaveChanges();
    }
    internal List<SalePlatform> GetAll() => Cache.GetOrCreate(CacheKey, () => dataBase.SalePlatforms.ToList());

    List<SalePlatform> ISalePlatformService.GetAll() => GetAll();
    SalePlatform ISalePlatformService.Get(Guid id) => dataBase.SalePlatforms.FirstOrDefault(s => s.Id == id);
}