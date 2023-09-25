using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.AsyncDataServices
{
    public interface ICardStorageServiceClient
    {
        Task<bool> SendCardToStorage(CreateCardDTO createCard, CancellationToken cancellationToken = default);
    }
}
