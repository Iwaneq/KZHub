using KZHub.CardGenerationService.DTOs.Card;
using System.Drawing;

namespace KZHub.CardGenerationService.Services.CardProcessing.Interfaces
{
    public interface ICardGenerator
    {
        Bitmap GenerateCard(CreateCardDTO cardDTO);
    }
}
