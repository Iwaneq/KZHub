using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KZHub.CardGenerationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardGenerationController : ControllerBase
    {
        [HttpGet]
        public IActionResult TestConnection()
        {
            Console.WriteLine("--> Testing connection");

            return Ok();
        }
    }
}
