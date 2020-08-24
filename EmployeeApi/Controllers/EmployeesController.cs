using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace EmployeeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Employee> Get()
        {
            return new List<Employee>
            {
                new Employee { Id = 1, Name = "Magic Johnson"},
                new Employee { Id = 2, Name = "Larry Bird"},
                new Employee { Id = 3, Name = "Michael Jordan"}
            };
        }
    }
}
