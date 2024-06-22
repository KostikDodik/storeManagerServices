using Model.Database;
using Model.Extentions;

namespace Model.Requests;

public class AvailableProduct: Product
{
    public AvailableProduct()
    {
        
    }
    
    public AvailableProduct(Product product)
    {
        this.CopyPossibleProperties(product);
    }
    
    public AvailableProduct(Product product, int available)
    {
        this.CopyPossibleProperties(product);
        Available = available;
    }
    
    public int Available { get; set; }
}