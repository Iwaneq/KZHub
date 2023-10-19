using AutoMapper;
using KZHub.CardStoringService.DTOs;
using KZHub.CardStoringService.Models;
using KZHub.CardStoringService.Repositories;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;

namespace KZHub.CardStoringService.AsyncDataServices
{
    public class SaveCardProcessor : ISaveCardProcessor
    {
        private readonly ICardDataService _cardDataService;
        private readonly IMapper _mapper;

        public SaveCardProcessor(ICardDataService cardDataService, IMapper mapper)
        {
            _cardDataService = cardDataService;
            _mapper = mapper;
        }

        public SaveCardStateDTO SaveCard(BasicDeliverEventArgs ea)
        {
            CreateCardDTO? cardDTO = DeserializeData(ea);

            Console.WriteLine($"--> Consumer Reciving... {cardDTO}");
            if (cardDTO is not null)
            {
                try
                {
                    var card = _mapper.Map<Card>(cardDTO);
                    _cardDataService.SaveCard(card);

                    return GetSaveState(card);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> EXCEPTION WAS THROWN WHILE SAVING CARD: {ex.Message}");

                    return GetSaveState(error: ex.Message);
                }
            }

            throw new ArgumentNullException(nameof(cardDTO), "Couldn't deserialize data or the data was null");
        }

        private CreateCardDTO? DeserializeData(BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            return JsonSerializer.Deserialize<CreateCardDTO>(notificationMessage);
        }

        private SaveCardStateDTO GetSaveState(Card? card = null, string? error = null)
        {
            if(error is not null)
            {
                return new SaveCardStateDTO()
                {
                    Error = error,
                    IsSaved = false
                };
            }
            else if(card is not null)
            {
                return new SaveCardStateDTO()
                {
                    IsSaved = true,
                    CardId = card.Id,
                };
            }

            return new SaveCardStateDTO()
            {
                IsSaved = false,
                Error = "Uncaught error while saving card"
            };
        }
    }
}
