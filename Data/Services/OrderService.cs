using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;
using Model.Requests;

namespace Data.Services;

public interface IOrderService
{
    List<OrderResponse> GetAll();
    Order Get(Guid id);
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

        if (existing.State != order.State)
        {
            order.UpdatedState ??= DateTime.UtcNow;
        }

        existing.CopyPossibleProperties(order);
        dataBase.Orders.Update(existing);
        dataBase.SaveChanges();
        HandleOrderItems(order, true);
        dataBase.SaveChanges();
    }

    void IOrderService.Delete(Order order)
    {
        dataBase.Orders.Remove(order);
        dataBase.SaveChanges();
    }

    void IOrderService.Delete(Guid id)
    {
        var order = dataBase.Orders.FirstOrDefault(p => p.Id == id);
        if (order != null)
        {
            itemService.RemoveOrder(id);
            dataBase.Orders.Remove(order);
            dataBase.SaveChanges();
        }
    }

    private record DbSumByOrder(Guid? OrderId, decimal TotalSum, decimal Income);

    List<OrderResponse> IOrderService.GetAll()
    {
        var orders = dataBase.Orders.ToList();
        var query = dataBase.Items.Where(i => i.OrderId != null).GroupBy(i => i.OrderId)
            .Select(g => new DbSumByOrder(
                g.Key,
                g.Sum(i => i.SalePrice),
                g.Sum(i => i.SalePrice - i.DeliveryPrice - i.SupplyPrice)));
        var items = query.ToList().ToDictionary(i => i.OrderId);
        
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

    Order IOrderService.Get(Guid id) => dataBase.Orders.FirstOrDefault(s => s.Id == id);
}