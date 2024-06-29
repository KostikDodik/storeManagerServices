namespace Model.Requests;

public class SupplyStats
{
    public decimal BoughtSum { get; set; }
    public int BoughtCount { get; set; }
    public decimal ReceivedSum { get; set; }
    public int ReceivedCount { get; set; }
}