namespace Model.Requests;

public class SalesByProduct
{
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
    public decimal SalesCount { get; set; }
    public decimal Income { get; set; }
    public decimal NetProfit { get; set; }
}