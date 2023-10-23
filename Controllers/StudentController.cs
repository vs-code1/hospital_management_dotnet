using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using DataTable = Management.Models.DataTable;

namespace Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IConfiguration _config;

        public StudentController(ManagementContext managementContext, IConfiguration configuration) {
            _context = managementContext;
            _config = configuration;
        }

        [HttpGet, Authorize(Roles = "Teacher")]
        public async Task<ActionResult<DataTable>> GetAllStudent()
        {
            return Ok(_context.Data.Where(x => x.Role == "student"));
        }
    }
}
