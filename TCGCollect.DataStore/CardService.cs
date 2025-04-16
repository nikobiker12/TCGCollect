using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCGCollect.Core;
using TCGCollect.DataStore;

namespace TCGCollect.Services
{
    public interface ICardService
    {
        Task<Card[]> GetCardsAsync(string filter);
    }


    public class CardService(InMemoryCardStore cardStore) : ICardService
    {
        private readonly InMemoryCardStore _cardStore = cardStore ?? throw new ArgumentNullException(nameof(cardStore));

        public Task<Card[]> GetCardsAsync(string filter)
        {
            return _cardStore.Search(filter);
        }
    }
}
