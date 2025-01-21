namespace Model.Database;

public enum ItemState
{
    Available = 0,
    Ordered = 1,
    Paid = 2,
    Finished = 3
}

public enum ItemListOrder
{
    SupplyDateDescending = 0,
    SupplyDateAscending = 1,
    StatusDescending = 2,
    StatusAscending = 3,
    BBDDescending = 4,
    BBDAscending = 5
}