using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Model.Database;
using Model.Requests;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class SuppliesController: ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAllSupplies()
    {
        using var dataService = new DataService();
        return Ok(dataService.Supplies.GetSupplies());
    }

    [HttpGet("{id}")]
    public IActionResult GetSupply(Guid id)
    {
        using var dataService = new DataService();
        return Ok(dataService.Supplies.GetSupply(id));
    }

    [HttpPost]
    public IActionResult AddSupply(SupplyRequest supply)
    {
        using var dataService = new DataService();
        dataService.Supplies.AddSupply(supply);
        return CreatedAtAction("GetSupply", new { id = supply.Id }, supply);
    }
    
    [HttpPut]
    public IActionResult UpdateSupply(SupplyRequest supply)
    {
        using var dataService = new DataService();
        dataService.Supplies.UpdateSupply(supply);
        return Ok(supply);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSupply(Guid id)
    {
        using var dataService = new DataService();
        dataService.Supplies.DeleteSupply(id);
        return NoContent();
    }
}