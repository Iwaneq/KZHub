using KZHub.CardGenerationService.DTOs.Card;
using SkiaSharp;

namespace KZHub.CardGenerationService.Services.CardProcessing.Interfaces
{
    public interface ICardGenerator
    {
        SKBitmap GenerateCard(CreateCardDTO cardDTO);
    }
}
