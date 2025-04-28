using Model.Database;
using Model.Requests;

namespace Data.Services;

internal partial class OrderService
{
    private void HandleChecks(OrderRequest order, Dictionary<Guid, List<Item>> existingItems = null)
    {
        var existingChecks = existingItems != null 
            ? checkService.GetByOrder(order.Id)
            : null;
        foreach (var check in order.Checks)
        {
            check.OrderId = order.Id;
            var existing = existingChecks?.FirstOrDefault(ch => ch.Id == check.Id);
            if (existing == null)
            {
                AddNewCheck(check);
            }
            else
            {
                UpdateExistingCheck(check);
            }
        }

        var currentIds = order.Checks.Select(c => c.Id).ToList();
        var forRemoval = existingChecks?.Where(c => !currentIds.Contains(c.Id)).ToList();
        if (forRemoval?.Count > 0)
        {
            checkService.RemoveRange(forRemoval);
        }
        CalculateNet(order);
    }

    private void AddNewCheck(Check newCheck)
    {
        checkService.Add(newCheck);
    }
    
    private void UpdateExistingCheck(Check existing)
    {
        checkService.Update(existing);
    }

    private static readonly Dictionary<PaymentType, decimal> NetCoefficient = new()
    {
        { PaymentType.Iban, 1 },
        { PaymentType.Card, 1 },
        { PaymentType.RozetkaPay, 0.99m },
        { PaymentType.NovaPay, 0.98m }
    };
    
    private void CalculateNet(OrderRequest order)
    {
        var checkBrutSum = order.Checks?.Sum(c => c.Sum);
        if (checkBrutSum is null or 0)
        {
            // No checks
            foreach (var row in order.Rows)
            {
                row.NetSum = 0;
            }
            return;
        }
        var rowsSum = order.Rows?.Sum(r => r.Price * r.Quantity);
        if (rowsSum is null or 0 || Math.Floor(rowsSum.Value) > Math.Floor(checkBrutSum.Value))
        {
            foreach (var row in order.Rows)
            {
                row.NetSum = 0;
            }
            return;
        }

        var netCoefficient = order.Checks.Sum(c => NetCoefficient[c.PaymentType] * c.Sum) / rowsSum.Value;
        var commissions = commissionService.GetCommissionsForSalePlatform(order.SalePlatformId, 
            order.Rows.Select(r => r.ProductId).Distinct().ToList());
        foreach (var row in order.Rows)
        {
            row.NetSum = CalculateNetSum(netCoefficient, row.Price, commissions.GetValueOrDefault(row.ProductId));
        }
        
        // Items will be updated due to corresponding rows later
    }
    
    private static decimal CalculateNetSum(decimal netCoefficient, decimal price, List<CommissionSize> commissions)
    {
        var commission = commissions?.Where(c => c.PriceOver <= price)
                .OrderByDescending(c => c.PriceOver).FirstOrDefault()?.Commission;
        return commission > 0
            ? (netCoefficient - commission.Value / 100) * price
            : netCoefficient * price;
    }
}