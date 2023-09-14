using KZHub.CardStoringService.Models;

namespace KZHub.CardStoringService.Repositories
{
    public interface ICardDataService
    {
        public Task SaveCard(Card card);
    }
}
