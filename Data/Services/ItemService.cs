using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface IItemService
{
    void Add(Item item);
    void Update(Item item);
    void Update(ICollection<Item> item);
    void Delete(Item item);
    void Delete(Guid Id);
    List<Item> GetBySupply(Guid supplyId, Guid? productId = null);
    public List<Item> GetAvailableByProduct(params Guid [] ids);
    List<Item> GetByOrder(Guid saleId);
    List<AvailableItemCount> GetAllAvailableCount(List<Guid> ids = null);
    List<Item> GetPage(Guid? productId = null, bool? onlyAvailable = null, ItemListOrder order = ItemListOrder.SupplyDateDescending, int page = 0);
    int Count(Guid? productId = null, bool? onlyAvailable = null);
    void UpdateBBDate(List<Guid> ids, DateTime date);
    List<Item> GetExpiringItems();
}

internal class ItemService(DataDbContext dataBase) : IItemService
{
    public void Add(Item item)
    {
        item.ReceivedDate = item.UpdatedStatus = DateTime.UtcNow;
        dataBase.Items.Add(item);
    }
    public void Add(ICollection<Item> items)
    {
        var now = DateTime.UtcNow;
        foreach (var item in items)
        {
            Add(item);
        }
    }
    
    void IItemService.Add(Item item)
    {
        Add(item);
        dataBase.SaveChanges();
    }
    
    public void Update(Item item)
    {
        var existing = dataBase.Items.FirstOrDefault(c => c.Id == item.Id);
        if (existing == null)
        {
            return;
        }
        if (existing.State != item.State)
        {
            item.UpdatedStatus = DateTime.UtcNow;
        }
        existing.CopyPossibleProperties(item);
        dataBase.Items.Update(existing);
    }

    public void Update(ICollection<Item> items)
    {
        foreach (var item in items)
        {
            Update(item);
        }
    }

    void IItemService.Update(Item item)
    {
        Update(item);
        dataBase.SaveChanges();
    }

    void IItemService.Update(ICollection<Item> items)
    {
        Update(items);
        dataBase.SaveChanges();
    }

    public void Delete(Item item)
    {
        item = dataBase.Items.FirstOrDefault(p => p.Id == item.Id);
        if (item == null)
        {
            return;
        }
        if (item.State != ItemState.Available)
        {
            throw new InvalidOperationException($"Item {item.Id} is already ordered!");
        }
        dataBase.Items.Remove(item);
    }

    internal void Delete(List<Item> items)
    {
        /*var ids = items.Select(x => x.Id).ToList();
        var dbItems = dataBase.Items.Where(i => ids.Contains(i.Id)).ToList();*/
        Guid invalidItemId = default;
        if (items.Any(item =>
            {
                invalidItemId = item.Id;
                return item.State != ItemState.Available;
            }))
        {
            throw new InvalidOperationException($"Item {invalidItemId} is already ordered!");
        }
        dataBase.Items.RemoveRange(items);
    }

    void IItemService.Delete(Item item)
    {
        Delete(item);
        dataBase.SaveChanges();
    }
    
    internal void Delete(Guid id)
    {
        var item = dataBase.Items.FirstOrDefault(p => p.Id == id);
        Delete(item);
        dataBase.SaveChanges();
    }

    void IItemService.Delete(Guid id)
    {
        Delete(id);
        dataBase.SaveChanges();
    }

    internal void RemoveSupply(Guid supplyId)
    {
        Delete(GetBySupply(supplyId));
    }

    /// <summary>
    /// CLeans items from sold status for removed order
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns>List of products</returns>
    internal List<Guid> RemoveOrder(Guid orderId)
    {
        var items = GetByOrder(orderId);
        items.ForEach(item =>
        {
            item.OrderId = null;
            item.State = ItemState.Available;
            dataBase.Items.Update(item);
        });
        return items.Select(i => i.ProductId).Distinct().ToList();
    }

    public List<Item> GetBySupply(Guid supplyId, Guid? productId = null) => productId == null 
        ?  dataBase.Items.Where(i => i.SupplyId == supplyId).ToList()
        : dataBase.Items.Where(i => i.SupplyId == supplyId && i.ProductId == productId).ToList();

    public Dictionary<Guid, List<Item>> GetGroupedByOrder(Guid orderId) =>
        GroupByProduct(dataBase.Items.Where(i => i.OrderId == orderId).ToList());

    public List<Item> GetAvailableByProduct(params Guid [] ids) => 
        dataBase.Items.Where(i => i.State == ItemState.Available && ids.Contains(i.ProductId)).ToList();

    public Dictionary<Guid, List<Item>> GetAvailableGroupedByProduct(params Guid [] ids) => 
        GroupByProduct(dataBase.Items.Where(i => i.State == ItemState.Available && ids.Contains(i.ProductId)).ToList());

    private Dictionary<Guid, List<Item>> GroupByProduct(List<Item> items) => items
        .GroupBy(i => i.ProductId)
        .ToDictionary(
            g => g.Key, 
            g => g.OrderBy(i => i.SupplyPrice + i.DeliveryPrice).ToList());

    public List<AvailableItemCount> GetAllAvailableCount(List<Guid> ids)
    {
        var query = ids == null 
            ? dataBase.Items.Where(i => i.State == ItemState.Available) 
            : dataBase.Items.Where(i => ids.Contains(i.ProductId) && i.State == ItemState.Available);
        return query.GroupBy(i => i.ProductId).Select(i => new AvailableItemCount
        {
            ProductId = i.Key,
            State = ItemState.Available,
            Count = i.Count()
        }).ToList();
    }

    public List<Item> GetByOrder(Guid orderId) => 
        dataBase.Items.Where(i => i.OrderId == orderId).ToList();

    internal Dictionary<Guid, bool> CheckSoldOutForSupply(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return new Dictionary<Guid, bool>();
        }
        return dataBase.Items.Where(i => ids.Contains(i.SupplyId)).GroupBy(i => i.SupplyId)
            .Select(gr => new
            {
                Id = gr.Key,
                Value = gr.Any() && gr.All(i => i.State == ItemState.Finished)
            }).ToList().ToDictionary(o => o.Id, o => o.Value);
    }
    
    const int pageSize = 10;

    private IQueryable<Item> GetPageQuery(Guid? productId = null, bool? onlyAvailable = null)
    {
        IQueryable<Item> query = dataBase.Items;
        if (productId != null)
        {
            query = query.Where(i => i.ProductId == productId);
        }
        if (onlyAvailable != null)
        {
            query = onlyAvailable.Value
                ? query.Where(i => i.State == ItemState.Available)
                : query.Where(i => i.State != ItemState.Available);
        }

        return query;
    }

    public List<Item> GetPage(Guid? productId = null, bool? onlyAvailable = null, ItemListOrder order = ItemListOrder.SupplyDateDescending, int page = 0)
    {
        var query = GetPageQuery(productId, onlyAvailable);
        switch (order)
        {
            case ItemListOrder.SupplyDateDescending:
                query = query.OrderByDescending(i => i.ReceivedDate);
                break;
            case ItemListOrder.SupplyDateAscending:
                query = query.OrderBy(i => i.ReceivedDate);
                break;
            case ItemListOrder.StatusDescending:
                query = query.OrderByDescending(i => i.UpdatedStatus);
                break;
            case ItemListOrder.StatusAscending:
                query = query.OrderBy(i => i.UpdatedStatus);
                break;
            case ItemListOrder.BBDDescending:
                query = query.OrderByDescending(i => i.BBDate);
                break;
            case ItemListOrder.BBDAscending:
                query = query.OrderBy(i => i.BBDate);
                break;
        }
        query = query.Skip(page * pageSize).Take(pageSize);
        return query.ToList();
    }

    public int Count(Guid? productId = null, bool? onlyAvailable = null)
    {
        var query = GetPageQuery(productId, onlyAvailable);
        return query.Count();
    }

    public void UpdateBBDate(List<Guid> ids, DateTime bbDate)
    {
        dataBase.Items.Where(i => ids.Contains(i.Id))
            .ExecuteUpdate(s => s.SetProperty(i => i.BBDate, i => bbDate));
        dataBase.SaveChanges();
    }

    public List<Item> GetExpiringItems()
    {
        return dataBase.Items.Where(i => i.State == ItemState.Available && i.BBDate != null 
            && EF.Functions.DateDiffDay(DateTime.UtcNow, i.BBDate) < 123).OrderBy(i => i.BBDate).ToList();
    }
}