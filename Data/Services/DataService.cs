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
    
    private CommissionUtilityService _commissionUtility;
    private CommissionUtilityService commissionUtilityService => _commissionUtility ??= new CommissionUtilityService(dbContext);

    private CategoryService _categories;
    private CategoryService categories => _categories ??= new CategoryService(dbContext, commissionUtilityService);
    public ICategoryService Categories => categories;

    private ProductService _products;
    private ProductService products => _products ??= new ProductService(dbContext);
    public IProductService Products => products;

    private ItemService _items;
    private ItemService items => _items ??= new ItemService(dbContext);
    public IItemService Items => items;

    private SupplierService _suppliers;
    private SupplierService suppliers => _suppliers ??= new SupplierService(dbContext);
    public ISupplierService Suppliers => suppliers;
    

    private SupplyRowService _supplyRows;
    private SupplyRowService supplyRows => _supplyRows ??= new SupplyRowService(dbContext);

    private SupplyService _supplies;
    private SupplyService supplies => _supplies ??= new SupplyService(dbContext, supplyRows, items);
    public ISupplyService Supplies => supplies;

    private SalePlatformService _salePlatforms;
    private SalePlatformService salePlatforms => _salePlatforms ??= new SalePlatformService(dbContext, commissionUtilityService);
    public ISalePlatformService SalePlatforms => salePlatforms;
    
    private CheckService _checks;
    private CheckService checks => _checks ??= new CheckService(dbContext);
    public ICheckService Checks => checks;
    
    private CommissionService _commissions;
    private CommissionService commissions => _commissions ??= 
        new CommissionService(dbContext, categories, salePlatforms, commissionUtilityService);
    public ICommissionService Commissions => commissions;
    
    private OrderService _orders;
    private OrderService orders => _orders ??= new OrderService(dbContext, items, checks, commissions);
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