using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void SendCardToGenerate(CreateCardDTO createCard);
    }
}
