namespace Data.Services;

public class DataService : IDisposable, IAsyncDisposable
{
    private DataDbContext _dbContext;
    private DataDbContext dbContext
    {
        get
        {
            if (_dbContext == null)
            {
                _dbContext = new DataDbContext();
                _dbContext.Database.EnsureCreated();
            }
            return _dbContext;
        }
    }

    private CategoryService _categories;
    private CategoryService categories => _categories ??= new CategoryService(dbContext);
    public ICategoryService Categories => categories;

    private ProductService _products;
    private ProductService products => _products ??= new ProductService(dbContext);
    public IProductService Products => products;

    private ItemService _items;
    private ItemService items => _items ??= new ItemService(dbContext);
    public IItemService Items => items;
    

    private SupplyRowService _supplyRows;
    private SupplyRowService supplyRows => _supplyRows ??= new SupplyRowService(dbContext);

    private SupplyService _supplies;
    private SupplyService supplies => _supplies ??= new SupplyService(dbContext, supplyRows, items);
    public ISupplyService Supplies => supplies;

    private SalePlatformService _salePlatforms;
    private SalePlatformService salePlatforms => _salePlatforms ??= new SalePlatformService(dbContext);
    public ISalePlatformService SalePlatforms => salePlatforms;
    
    private OrderService _orders;
    private OrderService orders => _orders ??= new OrderService(dbContext, items);
    public IOrderService Orders => orders;
    
    private StatisticsService _statistics;
    private StatisticsService statistics => _statistics ??= new StatisticsService(dbContext);
    public IStatisticsService Statistics => statistics;
    

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
    }
}