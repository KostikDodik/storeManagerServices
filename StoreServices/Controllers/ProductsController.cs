using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Model.Database;
using Model.Requests;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllProducts()
    {
        using var dataService = new DataService();
        return Ok(dataService.Products.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(Guid id)
    {
        using var dataService = new DataService();
        return Ok(dataService.Products.Get(id));
    }

    [HttpPost]
    public IActionResult AddProduct(Product product)
    {
        using var dataService = new DataService();
        dataService.Products.Add(product);
        return CreatedAtAction("GetProduct", new { id = product.Id }, product);
    }

    [HttpPut]
    public IActionResult UpdateProduct(Product product)
    {
        using var dataService = new DataService();
        dataService.Products.Update(product);
        return CreatedAtAction("GetProduct", new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(Guid id)
    {
        using var dataService = new DataService();
        dataService.Products.Delete(id);
        return NoContent();
    }

    [HttpGet("available")]
    public IActionResult GetAvailableProducts([FromQuery(Name = "ids")] List<Guid> ids = null)
    {
        using var dataService = new DataService();
        var products = dataService.Products.GetAll(ids);
        var availableProducts = dataService.Items.GetAllAvailableCount(ids).ToDictionary(i => i.ProductId, i => i.Count);
        return Ok(products.Select(p => new AvailableProduct(p, availableProducts.GetValueOrDefault(p.Id, 0))).ToList());
    }
}