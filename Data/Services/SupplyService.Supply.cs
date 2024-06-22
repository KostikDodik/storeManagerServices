using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;
using Model.Requests;

namespace Data.Services;

internal partial class SupplyService
{
    internal void AddSupply(SupplyRequest supply)
    {
        supply.Date ??= DateTime.UtcNow;
        supply.UpdatedState ??= DateTime.UtcNow;
        var dbEntity = supply.ConvertToDbEntity<Supply>();
        dataBase.Supplies.Add(dbEntity);
        dataBase.SaveChanges();
        supply.Id = dbEntity.Id;
        UpdateRows(supply, true);
        HandleReceivedItems(dbEntity);
    }

    internal void UpdateSupply(SupplyRequest supply)
    {
        var existing = dataBase.Supplies.FirstOrDefault(s => s.Id == supply.Id);
        if (existing == null)
        {
            return;
        }
        if (existing.State != supply.State)
        {
            supply.UpdatedState ??= DateTime.UtcNow;
            if (existing.State == SupplyState.Received)
            {
                // Received by mistake. Should remove created Items
                itemService.RemoveSupply(supply.Id);
            }
        }
        existing.CopyPossibleProperties(supply);
        UpdateRows(supply);
        HandleReceivedItems(supply);
    }

    private void UpdateRows(Supply supply, bool newItem = false)
    {
        if (supply.Rows == null)
        {
            foreach (var row in rowService.GetBySupply(supply.Id))
            {
                rowService.Delete(row);
            }
            dataBase.SaveChanges();
            return;
        }
        
        var existingRows = newItem ? [] : rowService.GetBySupply(supply.Id);
        foreach (var row in supply.Rows)
        {
            var existingRow = existingRows.FirstOrDefault(r => r.Id == row.Id);
            if (existingRow != null)
            {
                existingRow.UpdateDbEntity(row);
            }
            else
            {
                row.SupplyId = supply.Id;
                rowService.Add(row);
            }
        }

        foreach (var existingRow in existingRows.Where(existingRow => supply.Rows.All(r => r.Id != existingRow.Id)))
        {
            rowService.Delete(existingRow);
        }
    }

    private void HandleReceivedItems(Supply supply)
    {
        if (supply.State != SupplyState.Received)
        {
            return;
        }

        var existingDic = itemService.GetBySupply(supply.Id).GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var row in supply.Rows)
        {
            var existing = existingDic.GetValueOrDefault(row.ProductId);
            var existingCount = existing?.Count ?? 0;
            var difference = row.Count - existingCount;
            if (difference > 0)
            {
                AddNewItems(row, difference);
            }
            else if (difference < 0)
            {
                RemoveSurplusItems(ref existing, difference);
            }

            UpdateExistingItems(existing, row, supply);
        }

        dataBase.SaveChanges();
    }

    private void UpdateExistingItems(List<Item> existing, SupplyRow row, Supply supply)
    {
        if (existing == null)
        {
            return;
        }

        foreach (var item in existing)
        {
            item.SupplyPrice = row.SupplyPrice;
            item.DeliveryPrice = row.DeliveryPrice;
            item.ReceivedDate = supply.UpdatedState;
            item.UpdatedStatus = supply.UpdatedState;
            dataBase.Items.Update(item);
        }
    }

    private void AddNewItems(SupplyRow row, int difference)
    {
        itemService.Add(Enumerable.Repeat(0, difference)
            .Select(x => CreateItemFromRow(row))
            .ToList());
    }

    private void RemoveSurplusItems(ref List<Item> existing, int difference)
    {
        var removal = existing!.TakeLast(-difference).ToList();
        itemService.Delete(removal);
        existing = existing.Take(existing.Count + difference).ToList();
    }
    
    private void UpdateItemDetails(IEnumerable<Item> items, 
        ItemState state = ItemState.Available, 
        Guid? orderId = null, 
        decimal price = 0)
    {
        foreach (var item in items)
        {
            item.State = state;
            item.OrderId = orderId;
            item.SalePrice = price;
            dataBase.Items.Update(item);
        }
    }
    
    private Item CreateItemFromRow(SupplyRow row)
    {
        var item = new Item();
        item.CopyPossibleProperties(row);
        item.Id = default;
        return item;
    }

    internal void DeleteSupply(Supply supply)
    {
        if (supply == null)
        {
            throw new ArgumentNullException(nameof(supply));
        }

        var existingRows = rowService.GetBySupply(supply.Id);
        foreach (var row in existingRows)
        {
            rowService.Delete(row);
        }
        dataBase.Supplies.Remove(supply);
    }

    internal void DeleteSupply(Guid supplyId)
    {
        DeleteSupply(dataBase.Supplies.FirstOrDefault(s => s.Id == supplyId));
    }

    public Supply GetSupply(Guid supplyId)
    {
        return dataBase.Supplies.Include(s => s.Rows).FirstOrDefault(s => s.Id == supplyId);
    }
    
    void ISupplyService.AddSupply(SupplyRequest supply)
    {
        AddSupply(supply);
        dataBase.SaveChanges();
    }

    void ISupplyService.UpdateSupply(SupplyRequest supply)
    {
        UpdateSupply(supply);
        dataBase.SaveChanges();
    }

    void ISupplyService.DeleteSupply(Guid supplyId)
    {
        DeleteSupply(supplyId);
        dataBase.SaveChanges();
    }

    void ISupplyService.DeleteSupply(Supply supply)
    {
        DeleteSupply(dataBase.Supplies.FirstOrDefault(s => s.Id == supply.Id));
        dataBase.SaveChanges();
    }

    public List<Supply> GetSupplies(Guid? supplierId)
    {
        return supplierId == null
            ? dataBase.Supplies.ToList()
            : dataBase.Supplies.Where(s => s.SupplierId == supplierId).ToList();
    }
}