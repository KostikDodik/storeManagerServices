using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Model.Database;
using Model.Requests;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class OrdersController: ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAllOrders()
    {
        using var dataService = new DataService();
        return Ok(dataService.Orders.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(Guid id)
    {
        
        using var dataService = new DataService();
        var order = dataService.Orders.Get(id);
        if (order == null)
        {
            throw new KeyNotFoundException("There's no Sale with such an Id");
        }

        return Ok(new OrderRequest(order)
        {
            Rows = dataService.Items.GetByOrder(id).GroupBy(i => i.ProductId).Select(g => new OrderRow()
            {
                ProductId = g.Key,
                Quantity = g.Count(),
                Price = g.First().SalePrice
            }).ToList()
        });
    }

    [HttpPost]
    public IActionResult AddOrder(OrderRequest order)
    {
        using var dataService = new DataService();
        dataService.Orders.Add(order);
        return Ok(order);
    }
    
    [HttpPut]
    public IActionResult UpdateOrder(OrderRequest order)
    {
        using var dataService = new DataService();
        dataService.Orders.Update(order);
        return CreatedAtAction("GetOrder", new { id = order.Id }, order);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(Guid id)
    {
        using var dataService = new DataService();
        dataService.Orders.Delete(id);
        return NoContent();
    }
}