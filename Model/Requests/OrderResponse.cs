using Model.Database;

namespace Model.Requests;

public class OrderResponse(Order order) : OrderRequest(order)
{
    public decimal TotalCheck { get; set; }
    public decimal TotalIncome { get; set; }
}