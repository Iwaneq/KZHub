using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.AsyncDataServices
{
    public interface ICardGenerationServiceClient
    {
        Task<byte[]> SendCardToGenerate(CreateCardDTO createCard, CancellationToken cancellationToken = default);
    }
}
