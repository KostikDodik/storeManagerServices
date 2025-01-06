using Model.Database;
using Model.Extentions;
using Model.Requests;

namespace Data.Services;

public interface IOrderService
{
    List<OrderResponse> GetAll();
    OrderResponse Get(Guid id);
    Order Add(OrderRequest order);
    void Update(OrderRequest order);
    void Delete(Order order);
    void Delete(Guid saleId);
}

internal class OrderService(DataDbContext dataBase, ItemService itemService) : IOrderService
{
    private void HandleOrderItems(OrderRequest order, bool edit = false)
    {
        var availableDic = itemService.GetAvailableGroupedByProduct(order.Rows.Select(r => r.ProductId).ToArray());
        var existingDic = edit ? itemService.GetGroupedByOrder(order.Id) : null;
        foreach (var orderRow in order.Rows)
        {
            var existing = edit ? existingDic.GetValueOrDefault(orderRow.ProductId) : null;
            var existingCount = existing?.Count ?? 0;
            var difference = orderRow.Quantity - existingCount;
            if (difference > 0)
            {
                AddNewItemsToOrder(availableDic, orderRow, difference, order);
            }
            else if (difference < 0)
            {
                RemoveSurplusItems(ref existing, difference);
            }

            UpdateExistingItems(existing, order, orderRow);
        }
    }

    private void AddNewItemsToOrder(
        Dictionary<Guid, List<Item>> availableDic,
        OrderRow orderRow,
        int difference,
        OrderRequest order)
    {
        if (!availableDic.TryGetValue(orderRow.ProductId, out var available) || available.Count < difference)
        {
            throw new InvalidOperationException($"There are not enough of items for productId \"{orderRow.ProductId}\"");
        }

        UpdateItemDetails(available.Take(difference), order.State, order.Id, orderRow.Price);
    }

    private void RemoveSurplusItems(ref List<Item> existing, int difference)
    {
        UpdateItemDetails(existing!.TakeLast(-difference));
        existing = existing.TakeWhile(i => i.OrderId != null).ToList();
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

    private void UpdateExistingItems(List<Item> existing, OrderRequest order, OrderRow orderRow)
    {
        if (existing != null)
        {
            UpdateItemDetails(existing, order.State, order.Id, orderRow.Price);
        }
    }

    Order IOrderService.Add(OrderRequest order)
    {
        var dbOrder = order as Order;
        dbOrder.Date = order.Date ??= DateTime.UtcNow;
        dbOrder.UpdatedState = order.UpdatedState ??= DateTime.UtcNow;
        dbOrder.DateEdited = DateTime.UtcNow;
        dbOrder.Number = dataBase.Orders.Count(o => o.SalePlatformId == order.SalePlatformId) + 1;
        dataBase.Orders.Add(dbOrder);
        dataBase.SaveChanges();
        HandleOrderItems(order);
        dataBase.SaveChanges();
        return order;
    }

    void IOrderService.Update(OrderRequest order)
    {
        var existing = dataBase.Orders.FirstOrDefault(c => c.Id == order.Id);
        if (existing == null)
        {
            return;
        }

        order.DateEdited = DateTime.UtcNow;
        if (existing.State != order.State)
        {
            order.UpdatedState ??= DateTime.UtcNow;
        }

        var num = existing.Number;
        existing.CopyPossibleProperties(order);
        existing.Number = num;
        dataBase.Orders.Update(existing);
        dataBase.SaveChanges();
        HandleOrderItems(order, true);
        dataBase.SaveChanges();
    }
    
    internal void Delete(Order order)
    {
        var shift = dataBase.Orders.Where(o => o.SalePlatformId == order.SalePlatformId && o.Number > order.Number).ToList();
        foreach (var shiftedOrder in shift)
        {
            shiftedOrder.Number--;
        }
        itemService.RemoveOrder(order.Id);
        dataBase.Orders.Remove(order);
        dataBase.Orders.UpdateRange(shift);
        dataBase.SaveChanges();
    }

    void IOrderService.Delete(Order order) => Delete(order);

    void IOrderService.Delete(Guid id)
    {
        var order = dataBase.Orders.FirstOrDefault(p => p.Id == id);
        if (order != null)
        {
            Delete(order);
        }
    }

    private record DbSumByOrder(Guid? OrderId, decimal TotalSum, decimal Income);

    private Dictionary<Guid?, DbSumByOrder> getDbSumByOrders(List<Guid> ids = null)
    {
        var query = ids == null
            ? dataBase.Items.Where(i => i.OrderId != null)
            : dataBase.Items.Where(i => i.OrderId != null && ids.Contains(i.OrderId.Value));
        return query.GroupBy(i => i.OrderId)
            .Select(g => new DbSumByOrder(
                g.Key,
                g.Sum(i => i.SalePrice),
                g.Sum(i => i.SalePrice - i.DeliveryPrice - i.SupplyPrice))).ToList()
            .ToDictionary(i => i.OrderId);
    }
    

    List<OrderResponse> IOrderService.GetAll()
    {
        var orders = dataBase.Orders.OrderByDescending(o => o.Date).ToList();
        var items = getDbSumByOrders();
        
        return orders.Select(o =>
        {
            var sum = items.GetValueOrDefault(o.Id);
            return new OrderResponse(o)
            {
                TotalCheck = sum?.TotalSum ?? 0,
                TotalIncome = sum?.Income ?? 0
            };
        }).ToList();
    }

    OrderResponse IOrderService.Get(Guid id)
    {
        var order = dataBase.Orders.FirstOrDefault(s => s.Id == id);
        if (order == null)
        {
            return null;
        }
        var item = getDbSumByOrders([id]).GetValueOrDefault(id);
        return new OrderResponse(order)
        {
            TotalCheck = item?.TotalSum ?? 0,
            TotalIncome = item?.Income ?? 0,
            Rows = itemService.GetByOrder(id).GroupBy(i => i.ProductId).Select(g => new OrderRow
            {
                ProductId = g.Key,
                Quantity = g.Count(),
                Price = g.First().SalePrice
            }).ToList()
        };
    }
}