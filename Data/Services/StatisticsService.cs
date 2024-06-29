using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Requests;

namespace Data.Services;

public interface IStatisticsService
{
    List<SalesByProduct> GetSalesByProducts(DateTime? startDate = null, DateTime? endDate = null);
    SupplyStats GetSupplyStatistics(DateTime? startDate = null, DateTime? endDate = null);
}

internal class StatisticsService(DataDbContext dataBase) : IStatisticsService
{
    public List<SalesByProduct> GetSalesByProducts(DateTime? startDate, DateTime? endDate)
    {
        IQueryable<Item> query = dataBase.Items.Include(i => i.Product);
        if (startDate == null && endDate == null)
        {
            query = query.Where(i => i.State > ItemState.Available);
        }
        else
        {
            query = query.Include(i => i.Order).Where(i => i.State > ItemState.Available);
            if (startDate != null)
            {
                query = query.Where(i => i.Order.Date >= startDate);
            }

            if (endDate != null)
            {
                query = query.Where(i => i.Order.Date <= endDate);
            }
        }

        var grouped = query.GroupBy(i => new { i.Product.Id, i.Product.CategoryId, i.Product.Name })
            .Select(gr => new SalesByProduct
            {
                Name = gr.Key.Name,
                CategoryId = gr.Key.CategoryId,
                SalesCount = gr.Sum(i => 1),
                Income = gr.Sum(i => i.SalePrice),
                NetProfit = gr.Sum(i => i.SalePrice - i.SupplyPrice - i.DeliveryPrice)
            });
        return grouped.ToList();
    }

    public SupplyStats GetSupplyStatistics(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null && endDate == null)
        {
            return GroupSupplyStatsWithoutDateChecking(dataBase.SupplyRows).FirstOrDefault();
        }
        
        IQueryable<SupplyRow> query = dataBase.SupplyRows.Include(r => r.Supply);
        query = ApplyDateFilters(query, startDate, endDate);
        var groupQuery = GroupSupplyStatsWithDateChecking(query, startDate, endDate);
        var text = groupQuery.ToQueryString();
        return groupQuery.FirstOrDefault();
    }

    private IQueryable<SupplyRow> ApplyDateFilters(IQueryable<SupplyRow> query, DateTime? startDate, DateTime? endDate)
    {
        if (startDate != null)
        {
            query = query.Where(r =>
                r.Supply.Date >= startDate ||
                (r.Supply.State > SupplyState.SentToUkraine && r.Supply.UpdatedState >= startDate));
        }
        if (endDate != null)
        {
            query = query.Where(r =>
                r.Supply.Date <= endDate ||
                (r.Supply.State > SupplyState.SentToUkraine && r.Supply.UpdatedState <= endDate));
        }

        return query;
    }

    private IQueryable<SupplyStats> GroupSupplyStatsWithoutDateChecking(IQueryable<SupplyRow> query)
    {
        return query.GroupBy(r => 1)
            .Select(gr => new SupplyStats
            {
                BoughtCount = gr.Sum(row => row.Count),
                BoughtSum = gr.Sum(row => row.Count * (row.SupplyPrice + row.DeliveryPrice)),
                ReceivedCount = gr.Sum(row => row.Supply.State > SupplyState.SentToUkraine
                    ? row.Count
                    : 0),
                ReceivedSum = gr.Sum(row => row.Supply.State > SupplyState.SentToUkraine
                    ? row.Count * (row.SupplyPrice + row.DeliveryPrice)
                    : 0)
            });
    }

    private IQueryable<SupplyStats> GroupSupplyStatsWithDateChecking(IQueryable<SupplyRow> query, DateTime? startDate, DateTime? endDate)
    {
        return query.GroupBy(r => 1)
            .Select(gr => new SupplyStats
            {
                BoughtCount = gr.Sum(row =>
                    (startDate == null || row.Supply.Date >= startDate)
                    && (endDate == null || row.Supply.Date <= endDate)
                        ? row.Count
                        : 0),
                BoughtSum = gr.Sum(row =>
                    (startDate == null || row.Supply.Date >= startDate)
                    && (endDate == null || row.Supply.Date <= endDate)
                        ? row.Count * (row.SupplyPrice + row.DeliveryPrice)
                        : 0),
                ReceivedCount = gr.Sum(row =>
                    (startDate == null || (row.Supply.State > SupplyState.SentToUkraine && row.Supply.UpdatedState >= startDate))
                    && (endDate == null || (row.Supply.State > SupplyState.SentToUkraine && row.Supply.UpdatedState <= endDate))
                        ? row.Count
                        : 0),
                ReceivedSum = gr.Sum(row =>
                    (startDate == null || (row.Supply.State > SupplyState.SentToUkraine && row.Supply.UpdatedState >= startDate))
                    && (endDate == null || (row.Supply.State > SupplyState.SentToUkraine && row.Supply.UpdatedState <= endDate))
                        ? row.Count * (row.SupplyPrice + row.DeliveryPrice)
                        : 0)
            });
    }
}