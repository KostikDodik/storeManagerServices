using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Model.Database;
using Model.Requests;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class ItemsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetItems(
        [FromQuery(Name = "productId")] Guid? productId = null, 
        [FromQuery(Name = "onlyAvailable")] bool? onlyAvailable = null, 
        [FromQuery(Name = "sortOrder")] ItemListOrder? order = null, 
        [FromQuery(Name = "page")] int page = 1)
    {
        using var dataService = new DataService();
        return Ok(dataService.Items.GetPage(productId, onlyAvailable, order ?? ItemListOrder.SupplyDateDescending, page));
    }
    [HttpGet("count")]
    public IActionResult GetCount([FromQuery(Name = "productId")] Guid? productId = null, [FromQuery(Name = "onlyAvailable")] bool? onlyAvailable = null)
    {
        using var dataService = new DataService();
        return Ok(dataService.Items.Count(productId, onlyAvailable));
    }

    [HttpPost("bbdate")]
    public IActionResult ChangeBBDate([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] ChangeBBDateRequest data)
    {
        using var dataService = new DataService();
        dataService.Items.UpdateBBDate(data.ItemIds, data.BBDate);
        return Ok();
    }
    [HttpGet("supply")]
    public IActionResult GetForSupply([FromQuery(Name = "supplyId")] Guid supplyId)
    {
        using var dataService = new DataService();
        return Ok(dataService.Items.GetBySupply(supplyId));
    }
}