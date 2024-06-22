using Model.Database;
using Model.Extentions;

namespace Model.Requests;

public class SupplyRequest: Supply, IModelEntity
{
    public new DateTime? Date { get; set; } 
    public new DateTime? UpdatedState { get; set; } 
}