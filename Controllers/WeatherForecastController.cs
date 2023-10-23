using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Management.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetWeatherForecast")]
        public async Task<ActionResult> Get()
        {
            return Ok("Hii");
        }

        [HttpPost("uploadfile"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadProfile( IFormFile reqFile)
        {
            try
            {
                var file = reqFile;
                var folderName = Path.Combine("Resources", "Profile");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if(file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    return Ok(dbPath);
                }
                else
                {
                    return BadRequest();
                }

            }catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}"); 
            }
        }

        

    }
}