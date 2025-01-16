using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Model.Database;
using Model.Requests;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class SuppliesController(ProductsController productsController): ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAllSupplies()
    {
        using var dataService = new DataService();
        return Ok(dataService.Supplies.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetSupply(Guid id)
    {
        using var dataService = new DataService();
        return Ok(dataService.Supplies.Get(id));
    }

    [HttpPost]
    public IActionResult AddSupply(SupplyRequest supply)
    {
        using var dataService = new DataService();
        dataService.Supplies.Add(supply);
        return GetSupply(supply.Id);
    }
    
    [HttpPut]
    public IActionResult UpdateSupply(SupplyRequest supply)
    {
        using var dataService = new DataService();
        dataService.Supplies.Update(supply);
        return GetSupply(supply.Id);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSupply(Guid id)
    {
        using var dataService = new DataService();
        return productsController.GetAvailableProducts(dataService.Supplies.Delete(id));
    }
}