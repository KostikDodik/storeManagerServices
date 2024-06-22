using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface ISupplyRowService
{
    void Add(SupplyRow row);
    void Update(SupplyRow row);
    void Update(ICollection<SupplyRow> row);
    void Delete(SupplyRow row);
    void Delete(Guid Id);
    List<SupplyRow> GetBySupply(Guid supplyId);
}

internal class SupplyRowService(DataDbContext dataBase) : ISupplyRowService
{
    public void Add(SupplyRow row)
    {
        dataBase.SupplyRows.Add(row);
    }

    public void Add(ICollection<SupplyRow> rows)
    {
        foreach (var row in rows)
        {
            Add(row);
        }
    }

    void ISupplyRowService.Add(SupplyRow row)
    {
        Add(row);
        dataBase.SaveChanges();
    }

    public void Update(SupplyRow row)
    {
        var existing = dataBase.SupplyRows.FirstOrDefault(c => c.Id == row.Id);
        if (existing == null)
        {
            return;
        }
        existing.CopyPossibleProperties(row);
        dataBase.SupplyRows.Update(existing);
    }

    public void Update(ICollection<SupplyRow> rows)
    {
        foreach (var row in rows)
        {
            Update(row);
        }
    }

    void ISupplyRowService.Update(SupplyRow row)
    {
        Update(row);
        dataBase.SaveChanges();
    }

    void ISupplyRowService.Update(ICollection<SupplyRow> rows)
    {
        Update(rows);
        dataBase.SaveChanges();
    }

    public void Delete(SupplyRow row)
    {
        if (row != null)
        {
            dataBase.SupplyRows.Remove(row);
        }
    }

    void ISupplyRowService.Delete(SupplyRow row)
    {
        row = dataBase.SupplyRows.FirstOrDefault(p => p.Id == row.Id);
        Delete(row);
        dataBase.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var row = dataBase.SupplyRows.FirstOrDefault(p => p.Id == id);
        if (row != null)
        {
            dataBase.SupplyRows.Remove(row);
        }
    }

    public List<SupplyRow> GetBySupply(Guid supplyId) => dataBase.SupplyRows.Where(r => r.SupplyId == supplyId).ToList();

    void ISupplyRowService.Delete(Guid id)
    {
        Delete(id);
        dataBase.SaveChanges();
    }
}