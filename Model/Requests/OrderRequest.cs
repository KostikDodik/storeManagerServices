using Model.Database;
using Model.Extentions;

namespace Model.Requests;

public class OrderRequest: Order
{
    public OrderRequest() {}
    
    public OrderRequest(Order order)
    {
        this.CopyPossibleProperties(order);
        Checks = order.Checks?.ToList();
    }
    
    public new DateTime? Date { get; set; } 
    public new DateTime? UpdatedState { get; set; } 
    public List<OrderRow> Rows { get; set; }
    public new List<Check> Checks { get; set; }
}

public class OrderRow
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal NetSum { get; set; }
}