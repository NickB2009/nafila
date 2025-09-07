using Microsoft.AspNetCore.Mvc;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Organizations;

namespace Grande.Fila.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = BogusDataStore.GetAll<User>();
            return Ok(new { 
                Count = users.Count, 
                Users = users.Select(u => new { u.Id, u.FullName, u.Email, u.Role }) 
            });
        }

        [HttpGet("organizations")]
        public IActionResult GetOrganizations()
        {
            var organizations = BogusDataStore.GetAll<Organization>();
            return Ok(new { 
                Count = organizations.Count, 
                Organizations = organizations.Select(o => new { o.Id, o.Name, o.Slug, o.IsActive }) 
            });
        }

        [HttpPost("clear")]
        public IActionResult ClearData()
        {
            BogusDataStore.Clear();
            return Ok(new { Message = "All data cleared successfully" });
        }

        [HttpPost("initialize")]
        public IActionResult InitializeData()
        {
            BogusDataStore.Initialize();
            return Ok(new { Message = "Data initialized successfully" });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var userCount = BogusDataStore.GetAll<User>().Count;
            var orgCount = BogusDataStore.GetAll<Organization>().Count;
            
            return Ok(new { 
                UserCount = userCount, 
                OrganizationCount = orgCount,
                Message = "Singleton data store is working"
            });
        }
    }
} 