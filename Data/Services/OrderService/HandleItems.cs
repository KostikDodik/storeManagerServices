using Model.Database;
using Model.Requests;

namespace Data.Services;

internal partial class OrderService
{
    private void HandleOrderItems(OrderRequest order, Dictionary<Guid, List<Item>> existingItems = null)
    {
        var availableDic = itemService.GetAvailableGroupedByProduct(order.Rows.Select(r => r.ProductId).ToArray());
        var productIds = new List<Guid>();
        foreach (var orderRow in order.Rows)
        {
            var existing = existingItems?.GetValueOrDefault(orderRow.ProductId);
            var existingCount = existing?.Count ?? 0;
            var difference = orderRow.Quantity - existingCount;
            if (difference > 0)
            {
                AddNewItemsToOrder(availableDic, orderRow, difference, order);
            }
            else if (difference < 0)
            {
                RemoveSurplusItems(ref existing, -difference);
            }
            UpdateExistingItems(existing, order, orderRow);
            productIds.Add(orderRow.ProductId);
        }

        if (existingItems != null)
        {
            var removed = existingItems.Where(p => !productIds.Contains(p.Key))
                .SelectMany(p => p.Value).ToList();
            RemoveSurplusItems(ref removed);
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

        UpdateItemDetails(available.Take(difference), order.State, order.Id, orderRow.Price, orderRow.NetSum);
    }

    private void RemoveSurplusItems(ref List<Item> existing, int? difference = null)
    {
        UpdateItemDetails(existing!.TakeLast(difference ?? existing.Count));
        existing = existing.TakeWhile(i => i.OrderId != null).ToList();
    }

    private void UpdateItemDetails(IEnumerable<Item> items,
        ItemState state = ItemState.Available,
        Guid? orderId = null,
        decimal price = 0,
        decimal netSum = 0)
    {
        foreach (var item in items)
        {
            item.State = state;
            item.UpdatedStatus = DateTime.UtcNow;
            item.OrderId = orderId;
            item.SalePrice = price;
            item.NetSum = netSum;
            dataBase.Items.Update(item);
        }
    }

    private void UpdateExistingItems(List<Item> existing, OrderRequest order, OrderRow orderRow)
    {
        if (existing != null)
        {
            UpdateItemDetails(existing, order.State, order.Id, orderRow.Price, orderRow.NetSum);
        }
    }
}