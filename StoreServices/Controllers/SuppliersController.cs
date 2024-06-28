using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Model.Database;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class SuppliersController: ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAllSuppliers()
    {
        using var dataService = new DataService();
        return Ok(dataService.Suppliers.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetSupplier(Guid id)
    {
        using var dataService = new DataService();
        return Ok(dataService.Suppliers.Get(id));
    }

    [HttpPost]
    public IActionResult AddSupplier(Supplier supplier)
    {
        using var dataService = new DataService();
        dataService.Suppliers.Add(supplier);
        return CreatedAtAction("GetSupplier", new { id = supplier.Id }, supplier);
    }
    
    [HttpPut]
    public IActionResult UpdateSupplier(Supplier supplier)
    {
        using var dataService = new DataService();
        dataService.Suppliers.Update(supplier);
        return CreatedAtAction("GetSupplier", new { id = supplier.Id }, supplier);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSupplier(Guid id)
    {
        using var dataService = new DataService();
        dataService.Suppliers.Delete(id);
        return NoContent();
    }
}