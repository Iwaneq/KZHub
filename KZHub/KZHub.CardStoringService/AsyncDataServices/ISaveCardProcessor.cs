using KZHub.CardStoringService.DTOs;
using RabbitMQ.Client.Events;

namespace KZHub.CardStoringService.AsyncDataServices
{
    public interface ISaveCardProcessor
    {
        public SaveCardStateDTO SaveCard(BasicDeliverEventArgs ea);
    }
}
