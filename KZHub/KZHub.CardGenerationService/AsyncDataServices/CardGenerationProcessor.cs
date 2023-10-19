using KZHub.CardGenerationService.DTOs.Card;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;
using RabbitMQ.Client.Events;
using SkiaSharp;
using System.Text;
using System.Text.Json;

namespace KZHub.CardGenerationService.AsyncDataServices
{
    public class CardGenerationProcessor : ICardGenerationProcessor
    {
        private readonly ICardGenerator _generator;

        public CardGenerationProcessor(ICardGenerator generator)
        {
            _generator = generator;
        }

        public byte[] GenerateCardFromCreateCardDTO(BasicDeliverEventArgs ea)
        {
            CreateCardDTO? cardDto = DeserializeData(ea);

            Console.WriteLine($"--> Consumer Processing... {cardDto}");
            if (cardDto is not null)
            {
                SKBitmap card;

                try
                {
                    card = _generator.GenerateCard(cardDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> EXCEPTION WAS THROWN WHILE GENERATING CARD: {ex.Message}");
                    throw;
                }

                Console.WriteLine("--> Consumer Received");

                return EncodeCardToByteArray(card);
            }

            return new byte[0];
        }

        private byte[] EncodeCardToByteArray(SKBitmap card)
        {
            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                card.Encode(wstream, SKEncodedImageFormat.Png, 100);
                return memStream.ToArray();
            }
        }

        private CreateCardDTO? DeserializeData(BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            return JsonSerializer.Deserialize<CreateCardDTO>(notificationMessage);
        }
    }
}
