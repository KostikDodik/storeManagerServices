using Model.Database;
using Model.Extentions;
using Model.Requests;

namespace Data.Services;

public interface ISupplyService
{
    List<Supplier> GetSuppliers();
    Supplier GetSupplier(Guid id);
    void AddSupplier(Supplier supplier);
    void UpdateSupplier(Supplier supplier);
    void DeleteSupplier(Supplier supplier);
    void DeleteSupplier(Guid supplierId);
    List<SupplyResponse> GetSupplies(Guid? supplierId = null);
    SupplyResponse GetSupply(Guid supplyId);
    void AddSupply(SupplyRequest supply);
    void UpdateSupply(SupplyRequest supply);
    void DeleteSupply(Supply supply);
    void DeleteSupply(Guid supplyId);
}

internal partial class SupplyService(DataDbContext dataBase, SupplyRowService rowService, ItemService itemService) : ISupplyService
{
    void ISupplyService.AddSupplier(Supplier supplier)
    {
        dataBase.Suppliers.Add(supplier);
        dataBase.SaveChanges();
    }

    void ISupplyService.UpdateSupplier(Supplier supplier)
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

    void ISupplyService.DeleteSupplier(Supplier supplier)
    {
        if (dataBase.Supplies.Any(s => s.SupplierId == supplier.Id))
        {
            throw new InvalidOperationException("This supplier already has active supplies");
        }
        dataBase.Suppliers.Remove(supplier);
        dataBase.SaveChanges();
    }

    void ISupplyService.DeleteSupplier(Guid id)
    {
        var supplier = dataBase.Suppliers.FirstOrDefault(p => p.Id == id);
        if (supplier != null)
        {
            dataBase.Suppliers.Remove(supplier);
            dataBase.SaveChanges();
        }
    }

    List<Supplier> ISupplyService.GetSuppliers() => dataBase.Suppliers.ToList();
    Supplier ISupplyService.GetSupplier(Guid id) => dataBase.Suppliers.FirstOrDefault(s => s.Id == id);
}