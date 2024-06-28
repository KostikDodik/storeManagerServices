namespace Model.Database;

public enum SupplyState
{
    Paid = 0,
    DeliveredToStorage = 1,
    SentToUkraine = 2,
    Received = 3,
    SoldOut = 4
}