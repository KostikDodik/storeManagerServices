using Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class StatisticsController: ControllerBase
{
    [HttpGet("products")]
    public IActionResult GetAvailableProducts([FromQuery(Name = "start")] DateTime? dateStart, [FromQuery(Name = "end")] DateTime? dateEnd)
    {
        using var dataService = new DataService();
        var products = dataService.Statistics.GetSalesByProducts(dateStart, dateEnd);
        return Ok(products);
    }
}