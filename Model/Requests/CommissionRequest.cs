using Model.Database;
using Model.Extentions;

namespace Model.Requests;

public class CommissionRequest: CommissionCategory
{
    public new List<CommissionSize> CommissionSizes { get; set; }
    public List<CommissionSize> InheritedSizes { get; set; }
    
    public CommissionRequest(CommissionCategory category, List<CommissionSize> inheritedSizes = null)
    {
        this.CopyPossibleProperties(category);
        CommissionSizes = category.CommissionSizes?.ToList();
        InheritedSizes = inheritedSizes;
    }
}