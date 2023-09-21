using KZHub.CardStoringService.Data;
using KZHub.CardStoringService.Models;

namespace KZHub.CardStoringService.Repositories
{
    public class CardDataService : ICardDataService
    {
        private readonly DataContext _dataContext;

        public CardDataService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task SaveCard(Card card)
        {
            _dataContext.Cards.Add(card);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Card?> GetCard(int id)
        {
            return await _dataContext.Cards.FindAsync(id);
        }
    }
}
