using RabbitMQ.Client.Events;

namespace KZHub.CardGenerationService.AsyncDataServices
{
    public interface ICardGenerationProcessor
    {
        byte[] GenerateCardFromCreateCardDTO(BasicDeliverEventArgs ea);
    }
}
