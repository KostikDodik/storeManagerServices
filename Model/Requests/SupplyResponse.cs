using Model.Database;
using Model.Extentions;

namespace Model.Requests;

public class SupplyResponse: Supply, IModelEntity
{   
    public int SoldCount { get; set; }
    public decimal SoldMoney { get; set; }
}