using Microsoft.AspNetCore.Mvc;
using Data.Services;
using Model.Database;

namespace store_manager_backend.Controllers;

[ApiController, Route("[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllCategories()
    {
        using var dataService = new DataService();
        return Ok(dataService.Categories.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetCategory(Guid id)
    {
        using var dataService = new DataService();
        return Ok(dataService.Categories.Get(id));
    }

    [HttpPost]
    public IActionResult AddCategory(Category category)
    {
        using var dataService = new DataService();
        dataService.Categories.Add(category);
        return CreatedAtAction("GetCategory", new { id = category.Id }, category);
    }

    [HttpPut]
    public IActionResult UpdateCategory(Category category)
    {
        using var dataService = new DataService();
        dataService.Categories.Update(category);
        return CreatedAtAction("GetCategory", new { id = category.Id }, category);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(Guid id)
    {
        using var dataService = new DataService();
        dataService.Categories.Delete(id);
        return NoContent();
    }
}