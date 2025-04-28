using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;

namespace Data.Services;


public interface ICheckService
{
    void Add(Check check);
    
    void Update(Check check);
    
    void Delete(Check check);
    
    void Delete(Guid id);
    
    List<Check> GetAll();
    
    Check Get(Guid id);
}   

internal class CheckService(DataDbContext dataBase) : ICheckService
{
    internal void Add(Check check)
    {
        check.Id = Guid.Empty;
        dataBase.Checks.Add(check);
    }

    void ICheckService.Add(Check check)
    {
        Add(check);
        dataBase.SaveChanges();
    }

    internal void Update(Check check)
    {
        var existing = dataBase.Checks.FirstOrDefault(c => c.Id == check.Id);
        if (existing == null)
        {
            return;
        }
        existing.CopyPossibleProperties(check);
        dataBase.Checks.Update(existing);
    }

    void ICheckService.Update(Check check)
    {
        Update(check);
        dataBase.SaveChanges();
    }

    internal void Delete(Check check)
    {
        dataBase.Checks.Remove(check);
    }

    void ICheckService.Delete(Check check)
    {
        Delete(check);
        dataBase.SaveChanges();
    }

    internal void Delete(Guid id)
    {
        var check = dataBase.Checks.FirstOrDefault(p => p.Id == id);
        if (check != null)
        {
            Delete(check);
        }
    }

    void ICheckService.Delete(Guid id)
    {
        Delete(id);
        dataBase.SaveChanges();
    }

    internal List<Check> GetAll() => dataBase.Checks.ToList();

    List<Check> ICheckService.GetAll() => GetAll();

    internal Check Get(Guid id) => dataBase.Checks.FirstOrDefault(c => c.Id == id);

    Check ICheckService.Get(Guid id) => Get(id);
    
    public List<Check> GetByOrder(Guid orderId) => 
        dataBase.Checks.Where(c => c.OrderId == orderId).ToList();

    internal void RemoveRange(List<Check> checks)
    {
        dataBase.Checks.RemoveRange(checks);
    }
    
    internal void RemoveOrder(Guid orderId)
    {
        var checks = GetByOrder(orderId);
        RemoveRange(checks);
    }
}