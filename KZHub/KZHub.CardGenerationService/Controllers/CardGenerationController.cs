using KZHub.CardGenerationService.DTOs.Card;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KZHub.CardGenerationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardGenerationController : ControllerBase
    {
        private readonly ICardGenerator _generator;

        public CardGenerationController(ICardGenerator generator)
        {
            _generator = generator;
        }

        [HttpPost]
        public IActionResult GenerateCard(CreateCardDTO createCard)
        {
            try
            {
                var card = _generator.GenerateCard(createCard);

                if (card == null) throw new Exception("Card was null!");

                return Ok(card);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Exception occured while generating card: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet]
        public IActionResult TestConnection()
        {
            Console.WriteLine("--> Testing connection");

            return Ok();
        }
    }
}
