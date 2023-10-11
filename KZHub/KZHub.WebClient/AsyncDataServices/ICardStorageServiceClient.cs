using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.AsyncDataServices
{
    public interface ICardStorageServiceClient
    {
        Task<SaveCardStateDTO> SendCardToStorage(CreateCardDTO createCard, CancellationToken cancellationToken = default);
    }
}
