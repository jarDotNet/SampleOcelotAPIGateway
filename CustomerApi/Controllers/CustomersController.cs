using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return new List<Customer>
            {
                new Customer { Id = 1, Name = "Microsoft"},
                new Customer { Id = 2, Name = "Google"},
                new Customer { Id = 3, Name = "Apple"},
                new Customer { Id = 4, Name = "Amazon"},
                new Customer { Id = 5, Name = "IBM"}
            };
        }
    }
}
