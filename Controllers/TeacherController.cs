using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using DataTable = Management.Models.DataTable;

namespace Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IConfiguration _config;
        public static Guid TeacherId ;

        public TeacherController(ManagementContext context, IConfiguration config) {
            _context = context;
            _config = config;
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<DataTable>>> GetAllTeacher()
        {
            return Ok(_context.Data.Where(x => x.Role != "admin"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(string email)
        {
            var result = await _context.Data.FirstOrDefaultAsync(x => x.Email == email);
            if(result == null)
            {
                return NotFound("User Not Found!");
            }

            TeacherController.TeacherId = result.Id;
            var token = CreateToken(TeacherController.TeacherId);
            return Ok(token);
        }

        private string CreateToken(Guid id)
        {

            List<Claim> claims = new List<Claim>
            {
                new Claim("TeacherId", id.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpPost("addstudent"), Authorize(Roles = "Teacher"), DisableRequestSizeLimit]
        public async Task<ActionResult> AddStudent(string name, string phone, string email, string role ,IFormFile fileReq)
        {
            try
            {
                
                var check = await _context.Data.AnyAsync(x => x.Phone == phone);
                if (check)
                {
                    return Ok("Student Already Register!");
                } 
                if (name == null)
                {
                    return Ok("Student Value is not Correct!");
                }

                // new
                var dbPath = "";
                var file = fileReq;
                var folderName = Path.Combine("Resources", "Profile");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
                else
                {
                    return BadRequest();
                }
                Guid roleId = TeacherController.TeacherId;
                var user = new DataTable {
                    RoleId = roleId,
                    Id = Guid.NewGuid(),
                    ProfilePath = dbPath,
                    Name = name,
                    Phone = phone,
                    Email = email,
                    Role = role
                    };

                
                _context.Data.Add(user);
                await _context.SaveChangesAsync();
                return Ok("Student Added!");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
