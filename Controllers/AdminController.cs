using MailKit.Net.Smtp;
using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Authorization;

namespace Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        public readonly ManagementContext _context;
        public readonly IConfiguration _configuration;
        public static string otp = "";
        public static Guid UserId;
        public SchoolController(ManagementContext context, IConfiguration configuration)
        {
            Console.WriteLine("Hii");
            _context = context;
            _configuration = configuration;
        }


        [HttpPost]
        public async Task<ActionResult<DataTable>> AddData(DataTable request)
        {
            var check = await _context.Data.AnyAsync(x => x.Phone == request.Phone);
            if (check)
            {
                return Ok("User Already Found");
            }

            if (request == null)
            {
                return BadRequest("Value is Not valid");
            }

            SchoolController.UserId = Guid.NewGuid();
            request.Id = SchoolController.UserId;
            _context.Data.Add(request);
            await _context.SaveChangesAsync();
            return Ok("User is saved");
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<DataTable>>> GetAll()
        {
            return Ok(await _context.Data.ToArrayAsync());
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(string email)
        {
            SchoolController.otp = "";
            Random generator = new Random();
            string s = generator.Next(0, 1000000).ToString("D6");

            SchoolController.otp = s;
            var result = await _context.Data.FirstOrDefaultAsync(x => x.Email == email);
            if (result == null)
            {
                return NotFound("User Not Found!");
            }

            SchoolController.UserId = result.Id;

            SendMail2Step(email, "This is Subject", SchoolController.otp);
            return Ok("Check Your Mail");
        }

        [HttpPost("otp")]
        public async Task<ActionResult> Otp(string otp)
        {
            var token = CreateToken();
            if(SchoolController.otp == otp)
            {
                return Ok(token);
            }

            return BadRequest($"Wrong Otp!");

        }

        private string CreateToken()
        {

            List<Claim> claims = new List<Claim>
            {
                new Claim("UserId", SchoolController.UserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public static void SendMail2Step(string To, string subject, string body)
        {
            try
            {
                var smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };
                smtpClient.Credentials = new NetworkCredential("gmail", "pass"); //Use the new password, generated from google!
                var message = new System.Net.Mail.MailMessage(
                        new System.Net.Mail.MailAddress("bob772021@gmail.com", "Admin"),
                        new System.Net.Mail.MailAddress(To, To)
                    );
                message.Subject = subject;
                message.Body = body;

                smtpClient.Send(message);

            }catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("addteacher"), Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddTeacher(DataTable request)
        {
            var check = await _context.Data.AnyAsync(x => x.Phone == request.Phone);
            if (check)
            {
                return Ok("Already Found!");
            }

            request.Id = Guid.NewGuid();
            request.RoleId = SchoolController.UserId;
            _context.Data.Add(request);
            await _context.SaveChangesAsync();
            return Ok("Teacher Added!");
        }



    }
}
