using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.AsyncDataServices
{
    public interface IMessageBusClient
    {
        Task<byte[]> SendCardToGenerate(CreateCardDTO createCard, CancellationToken cancellationToken = default);
    }
}
