using Microsoft.AspNetCore.Mvc;
using Data.Services;
using Model.Database;

namespace store_manager_backend.Controllers
{
    [ApiController, Route("[controller]")]
    public class CommissionsController : ControllerBase
    {
        [HttpGet("categories/{id}")]
        public IActionResult GetForCategory(Guid id)
        {
            using var dataService = new DataService();
            return Ok(dataService.Commissions.GetCategory(id));
        }

        [HttpGet("{id}")]
        public IActionResult GetCommission(Guid id)
        {
            using var dataService = new DataService();
            return Ok(dataService.Commissions.Get(id));
        }

        [HttpPost]
        public IActionResult AddCategory(CommissionCategory commissionCategory)
        {
            using var dataService = new DataService();
            dataService.Commissions.Add(commissionCategory);
            return CreatedAtAction("GetCommission", new { id = commissionCategory.Id }, commissionCategory);
        }

        [HttpPut]
        public IActionResult UpdateCategory(CommissionCategory commissionCategory)
        {
            using var dataService = new DataService();
            dataService.Commissions.Update(commissionCategory);
            return CreatedAtAction("GetCommission", new { id = commissionCategory.Id }, commissionCategory);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(Guid id)
        {
            using var dataService = new DataService();
            dataService.Commissions.Delete(id);
            return NoContent();
        }
    }
}