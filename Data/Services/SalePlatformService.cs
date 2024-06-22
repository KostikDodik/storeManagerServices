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

internal class SalePlatformService(DataDbContext dataBase) : ISalePlatformService
{
    void ISalePlatformService.Add(SalePlatform salePlatform)
    {
        dataBase.SalePlatforms.Add(salePlatform);
        dataBase.SaveChanges();
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

    void ISalePlatformService.Delete(SalePlatform salePlatform)
    {
        if (dataBase.Orders.Any(s => s.SalePlatformId == salePlatform.Id))
        {
            throw new InvalidOperationException("This Sale Platform already has active sales");
        }
        dataBase.SalePlatforms.Remove(salePlatform);
        dataBase.SaveChanges();
    }

    void ISalePlatformService.Delete(Guid id)
    {
        var salePlatform = dataBase.SalePlatforms.FirstOrDefault(p => p.Id == id);
        if (salePlatform != null)
        {
            dataBase.SalePlatforms.Remove(salePlatform);
            dataBase.SaveChanges();
        }
    }

    List<SalePlatform> ISalePlatformService.GetAll() => dataBase.SalePlatforms.ToList();
    SalePlatform ISalePlatformService.Get(Guid id) => dataBase.SalePlatforms.FirstOrDefault(s => s.Id == id);
}