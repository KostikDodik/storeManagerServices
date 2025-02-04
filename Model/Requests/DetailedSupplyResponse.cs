using Model.Database;
using Model.Extentions;

namespace Model.Requests;

public class DetailedRow : SupplyRow, IModelEntity
{
    public int InStock { get; set; }
}
public class DetailedSupplyResponse: SupplyResponse
{
    public new List<DetailedRow> Rows { get; set; }
}