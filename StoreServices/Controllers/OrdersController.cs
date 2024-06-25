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
        var orders = dataService.Orders.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(Guid id)
    {
        
        using var dataService = new DataService();
        var order = dataService.Orders.Get(id);
        if (order == null)
        {
            throw new KeyNotFoundException("There's no order with such an Id");
        }
        return Ok(order);
    }

    [HttpPost]
    public IActionResult AddOrder(OrderRequest order)
    {
        using var dataService = new DataService();
        dataService.Orders.Add(order);
        return GetOrder(order.Id);
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