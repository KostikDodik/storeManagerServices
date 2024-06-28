using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Requests;

namespace Data.Services;

public interface IStatisticsService
{
    List<SalesByProduct> GetSalesByProducts(DateTime? startDate, DateTime? endDate);
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
                query = query.Where(i => i.Order.Date <= startDate);
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
}