using Microsoft.EntityFrameworkCore;
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
    /// <returns>List of removed products</returns>
    List<Guid> Delete(Order order);
    /// <returns>List of removed products</returns>
    List<Guid> Delete(Guid saleId);
}

internal partial class OrderService(DataDbContext dataBase, ItemService itemService, CheckService checkService, CommissionService commissionService) : IOrderService
{
    Order IOrderService.Add(OrderRequest order)
    {
        var dbOrder = order as Order;
        dbOrder.Date = order.Date ??= DateTime.UtcNow;
        dbOrder.UpdatedState = order.UpdatedState ??= DateTime.UtcNow;
        dbOrder.DateEdited = DateTime.UtcNow;
        dbOrder.Number = dataBase.Orders.Count(o => o.SalePlatformId == order.SalePlatformId) + 1;
        dataBase.Orders.Add(dbOrder);
        dataBase.SaveChanges();
        HandleChecks(order);
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
        var existingItems = itemService.GetGroupedByOrder(order.Id);
        HandleChecks(order, existingItems);
        HandleOrderItems(order, existingItems);
        dataBase.SaveChanges();
    }
    
    internal List<Guid> Delete(Order order)
    {
        var shift = dataBase.Orders.Where(o => o.SalePlatformId == order.SalePlatformId && o.Number > order.Number).ToList();
        foreach (var shiftedOrder in shift)
        {
            shiftedOrder.Number--;
        }
        var ids = itemService.RemoveOrder(order.Id);
        checkService.RemoveOrder(order.Id);
        dataBase.Orders.Remove(order);
        dataBase.Orders.UpdateRange(shift);
        dataBase.SaveChanges();
        return ids;
    }

    List<Guid> IOrderService.Delete(Order order) => Delete(order);

    List<Guid> IOrderService.Delete(Guid id)
    {
        var order = dataBase.Orders.FirstOrDefault(p => p.Id == id);
        return order != null ? Delete(order) : null;
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
                g.Sum(i => (i.NetSum > 0 ? i.NetSum : i.SalePrice)),
                g.Sum(i => (i.NetSum > 0 ? i.NetSum : i.SalePrice) - i.DeliveryPrice - i.SupplyPrice))).ToList()
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
        var order = dataBase.Orders.Include(o => o.Checks).FirstOrDefault(s => s.Id == id);
        if (order == null)
        {
            return null;
        }
        var item = getDbSumByOrders([id]).GetValueOrDefault(id);
        return new OrderResponse(order)
        {
            TotalCheck = item?.TotalSum ?? 0,
            TotalIncome = item?.Income ?? 0,
            Rows = itemService.GetByOrder(id).GroupBy(i => i.ProductId).Select(g =>
            {
                return new OrderRow
                {
                    ProductId = g.Key,
                    Quantity = g.Count(),
                    Price = g.First().SalePrice,
                    NetSum = g.First().NetSum
                };
            }).ToList()
        };
    }
}