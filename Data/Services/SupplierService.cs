using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface ISupplierService
{
    List<Supplier> GetAll();
    Supplier Get(Guid id);
    void Add(Supplier supplier);
    void Update(Supplier supplier);
    void Delete(Supplier supplier);
    void Delete(Guid supplierId);
}

internal class SupplierService(DataDbContext dataBase) : ISupplierService
{
    void ISupplierService.Add(Supplier supplier)
    {
        dataBase.Suppliers.Add(supplier);
        dataBase.SaveChanges();
    }

    void ISupplierService.Update(Supplier supplier)
    {
        var existing = dataBase.Suppliers.FirstOrDefault(c => c.Id == supplier.Id);
        if (existing == null)
        {
            return;
        }

        existing.CopyPossibleProperties(supplier);
        dataBase.Suppliers.Update(existing);
        dataBase.SaveChanges();
    }

    void ISupplierService.Delete(Supplier supplier)
    {
        if (dataBase.Supplies.Any(s => s.SupplierId == supplier.Id))
        {
            throw new InvalidOperationException("This supplier already has active supplies");
        }
        dataBase.Suppliers.Remove(supplier);
        dataBase.SaveChanges();
    }

    void ISupplierService.Delete(Guid id)
    {
        var supplier = dataBase.Suppliers.FirstOrDefault(p => p.Id == id);
        if (supplier != null)
        {
            dataBase.Suppliers.Remove(supplier);
            dataBase.SaveChanges();
        }
    }

    List<Supplier> ISupplierService.GetAll() => dataBase.Suppliers.ToList();
    Supplier ISupplierService.Get(Guid id) => dataBase.Suppliers.FirstOrDefault(s => s.Id == id);
}