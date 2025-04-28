using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;
using Model.Requests;

namespace Data.Services;

public interface ISupplyService
{
    List<SupplyResponse> GetAll(Guid? supplierId = null);
    DetailedSupplyResponse Get(Guid supplyId);
    void Add(SupplyRequest supply);
    void Update(SupplyRequest supply);
    List<Guid> Delete(Supply supply);
    List<Guid> Delete(Guid supplyId);
}

internal class SupplyService(DataDbContext dataBase, SupplyRowService rowService, ItemService itemService) : ISupplyService
{
    internal void AddSupply(SupplyRequest supply)
    {
        supply.Date ??= DateTime.UtcNow;
        supply.UpdatedState ??= DateTime.UtcNow;
        supply.DateEdited = DateTime.UtcNow;
        var dbEntity = supply.ConvertToDbEntity<Supply>();
        dbEntity.Number = dataBase.Supplies.Count(s => s.SupplierId == supply.SupplierId) + 1;
        dataBase.Supplies.Add(dbEntity);
        dataBase.SaveChanges();
        supply.Id = dbEntity.Id;
        UpdateRows(supply, true);
        HandleReceivedItems(dbEntity);
    }

    internal void UpdateSupply(SupplyRequest supply)
    {
        var existing = dataBase.Supplies.Include(s => s.Rows).FirstOrDefault(s => s.Id == supply.Id);
        if (existing == null)
        {
            return;
        }
        supply.DateEdited = DateTime.UtcNow;

        if (existing.State != supply.State)
        {
            supply.UpdatedState ??= DateTime.UtcNow;
            if (existing.State == SupplyState.Received)
            {
                // Received by mistake. Should remove created Items
                itemService.RemoveSupply(supply.Id);
            }
        }

        var num = existing.Number;
        HandleReceivedItems(supply, existing);
        existing.CopyPossibleProperties(supply);
        existing.Number = num;
        UpdateRows(supply);
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
            row.SupplyId = supply.Id;
            if (existingRow != null)
            {
                existingRow.UpdateDbEntity(row);
            }
            else
            {
                rowService.Add(row);
            }
        }

        foreach (var existingRow in existingRows.Where(existingRow => supply.Rows.All(r => r.Id != existingRow.Id)))
        {
            rowService.Delete(existingRow);
        }
    }

    private void HandleReceivedItems(Supply supply, Supply beforeUpdate = null)
    {
        if (supply.State != SupplyState.Received)
        {
            return;
        }

        var existingDic = itemService.GetBySupply(supply.Id).GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var productIds = new List<Guid>();
        foreach (var row in supply.Rows)
        {
            var existing = existingDic.GetValueOrDefault(row.ProductId);
            if (beforeUpdate != null && row.Id != default)
            {
                var rowBeforeUpdate = beforeUpdate.Rows.FirstOrDefault(r => r.Id == row.Id);
                if (rowBeforeUpdate != null)
                {
                    existing = existing?.Where(i => i.SupplyPrice == rowBeforeUpdate.SupplyPrice && i.DeliveryPrice == rowBeforeUpdate.DeliveryPrice).ToList();
                }
            }
            var existingCount = existing?.Count ?? 0;
            var difference = row.Count - existingCount;
            if (difference > 0)
            {
                row.SupplyId = supply.Id;
                AddNewItems(row, difference);
            }
            else if (difference < 0)
            {
                RemoveSurplusItems(ref existing, difference);
            }

            UpdateExistingItems(existing, row, supply);
            productIds.Add(row.ProductId);
        }
        itemService.Delete(existingDic.Where(p => !productIds.Contains(p.Key))
            .SelectMany(p => p.Value).ToList());

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

    private Item CreateItemFromRow(SupplyRow row)
    {
        var item = new Item();
        item.CopyPossibleProperties(row);
        item.Id = default;
        return item;
    }

    /// <summary>
    /// Deletes the specified supply along with its associated supply rows, updates the numbering of subsequent supplies for the same supplier,
    /// and removes the supply from the database.
    /// </summary>
    /// <param name="supply">The supply to be deleted. Cannot be null.</param>
    /// <returns>A list of product IDs of the deleted supply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided supply is null.</exception>
    internal List<Guid> DeleteSupply(Supply supply)
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
        itemService.RemoveSupply(supply.Id);
        
        var shift = dataBase.Supplies.Where(s => s.SupplierId == supply.SupplierId && s.Number > supply.Number).ToList();
        foreach (var shiftedSupply in shift)
        {
            shiftedSupply.Number--;
        }
        dataBase.Supplies.Remove(supply);
        dataBase.Supplies.UpdateRange(shift);
        return supply.State == SupplyState.Received || supply.State == SupplyState.SoldOut 
            ? existingRows.Select(r => r.ProductId).ToList()
            : null;
    }

    internal List<Guid> DeleteSupply(Guid supplyId)
    {
        return DeleteSupply(dataBase.Supplies.FirstOrDefault(s => s.Id == supplyId));
    }


    private record DbSumForSupply(
        Guid SupplyId,
        int SoldCount,
        decimal SoldMoney);

    private Dictionary<Guid, DbSumForSupply> GetSupplyItemsInfo(List<Guid> ids = null)
    {
        var query = ids == null ? dataBase.Items : dataBase.Items.Where(i => ids.Contains(i.SupplyId)); 
        var q = query.GroupBy(i => i.SupplyId)
            .Select(g => new DbSumForSupply(
                g.Key,
                g.Sum(i => i.OrderId == null ? 0 : 1),
                g.Sum(i => (i.NetSum > 0 ? i.NetSum : i.SalePrice)))
            );
        return q.ToList().ToDictionary(i => i.SupplyId);
    }

    public DetailedSupplyResponse Get(Guid supplyId)
    {
        var dbSupply = dataBase.Supplies
            .Include(s => s.Rows)
            .Include(s => s.Items)
            .FirstOrDefault(s => s.Id == supplyId);
        if (dbSupply == null)
        {
            return null;
        }
        UpdateSoldOutState([dbSupply]);
        
        var supply = dbSupply.ConvertFromDbEntity<DetailedSupplyResponse>();
        
        var sum = new DbSumForSupply(supplyId,
            dbSupply.Items.Sum(i => i.OrderId == null ? 0 : 1),
            dbSupply.Items.Sum(i => (i.NetSum > 0 ? i.NetSum : i.SalePrice))
        );
        supply.CopyPossibleProperties(sum);
        supply.Rows = dbSupply.Rows.Select(dbRow =>
        {
            var row = dbRow.ConvertFromDbEntity<DetailedRow>();
            row.InStock = dbSupply.Items.Where(i => i.ProductId == dbRow.ProductId).Count(i => i.OrderId == null);
            return row;
        }).ToList();
        return supply;
    }

    void ISupplyService.Add(SupplyRequest supply)
    {
        AddSupply(supply);
        dataBase.SaveChanges();
    }

    void ISupplyService.Update(SupplyRequest supply)
    {
        UpdateSupply(supply);
        dataBase.SaveChanges();
    }


    /// <summary>
    /// Deletes the specified supply along with its associated supply rows, updates the numbering of subsequent supplies for the same supplier,
    /// and removes the supply from the database.
    /// </summary>
    /// <returns>A list of product IDs of the deleted supply.</returns>
    List<Guid> ISupplyService.Delete(Guid supplyId)
    {
        var list = DeleteSupply(supplyId);
        dataBase.SaveChanges();
        return list;
    }


    /// <summary>
    /// Deletes the specified supply along with its associated supply rows, updates the numbering of subsequent supplies for the same supplier,
    /// and removes the supply from the database.
    /// </summary>
    /// <param name="supply">The supply to be deleted. Cannot be null.</param>
    /// <returns>A list of product IDs of the deleted supply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided supply is null.</exception>
    List<Guid> ISupplyService.Delete(Supply supply)
    {
        var list = DeleteSupply(dataBase.Supplies.FirstOrDefault(s => s.Id == supply.Id));
        dataBase.SaveChanges();
        return list;
    }

    public List<SupplyResponse> GetAll(Guid? supplierId)
    {
        // TODO: Remove supply rows from here and move soldoutstate to suply and order save.
        var supplies = supplierId == null
            ? dataBase.Supplies.Include(s => s.Rows).OrderByDescending(s => s.Date).ToList()
            : dataBase.Supplies.Include(s => s.Rows).OrderByDescending(s => s.Date).Where(s => s.SupplierId == supplierId).ToList();
        UpdateSoldOutState(supplies);
        var ids = supplierId == null ? null : supplies.Select(s => s.Id).ToList();
        var info = GetSupplyItemsInfo(ids);
        return supplies.Select(s =>
        {
            var supply = s.ConvertFromDbEntity<SupplyResponse>();
            supply.Rows = s.Rows;
            var sum = info.GetValueOrDefault(s.Id);
            if (sum != null)
            {
                supply.CopyPossibleProperties(sum);
            }
            return supply;
        }).ToList();
    }

    private void UpdateSoldOutState(List<Supply> supplies)
    {
        supplies = supplies.Where(s => s.State != SupplyState.SoldOut).ToList();
        var fetchedRows = supplies.Where(s => s.Items != null).ToDictionary(s => s.Id, s => s.Items.ToList());
        var supplyIds = supplies.Where(s => s.Items == null).Select(s => s.Id).ToList();
        var soldOutItems = itemService.CheckSoldOutForSupply(supplyIds, fetchedRows);
        foreach (var supply in supplies)
        {
            if (soldOutItems.TryGetValue(supply.Id, out var soldOut) && soldOut)
            {
                supply.State = SupplyState.SoldOut;
                dataBase.Supplies.Update(supply);
            }
        }
        dataBase.SaveChanges();
    }
}