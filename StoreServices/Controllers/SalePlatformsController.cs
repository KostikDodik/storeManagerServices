using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Model.Database;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class SalePlatformsController: ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAllSalePlatforms()
    {
        using var dataService = new DataService();
        return Ok(dataService.SalePlatforms.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetSalePlatform(Guid id)
    {
        using var dataService = new DataService();
        return Ok(dataService.SalePlatforms.Get(id));
    }

    [HttpPost]
    public IActionResult AddSalePlatform(SalePlatform salePlatform)
    {
        using var dataService = new DataService();
        dataService.SalePlatforms.Add(salePlatform);
        return CreatedAtAction("GetSalePlatform", new { id = salePlatform.Id }, salePlatform);
    }
    
    [HttpPut]
    public IActionResult UpdateSalePlatform(SalePlatform salePlatform)
    {
        using var dataService = new DataService();
        dataService.SalePlatforms.Update(salePlatform);
        return CreatedAtAction("GetSalePlatform", new { id = salePlatform.Id }, salePlatform);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSalePlatform(Guid id)
    {
        using var dataService = new DataService();
        dataService.SalePlatforms.Delete(id);
        return NoContent();
    }
}