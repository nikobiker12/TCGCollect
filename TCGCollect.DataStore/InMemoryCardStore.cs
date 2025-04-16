using System.Text.Json;
using FluentStorage;
using TCGCollect.Core;

namespace TCGCollect.DataStore
{
    public class InMemoryCardStore(CardStoreConfiguration cardStoreConfiguration)
    {
        private readonly CardStoreConfiguration _cardStoreConfiguration = cardStoreConfiguration ?? throw new ArgumentNullException(nameof(cardStoreConfiguration));
        private readonly List<Card> _cards = new();

        public async Task Seed(CancellationToken cancellationToken = default)
        {
            // Create a storage provider for the folder/container
            var storage = StorageFactory.Blobs.FromConnectionString(_cardStoreConfiguration.ConnectionString);

            // Retrieve all JSON files from the folder/container
            var jsonFiles = await storage.ListAsync(new FluentStorage.Blobs.ListOptions
            {
                FilePrefix = "cards" // Filter to only include JSON files
            }, cancellationToken);

            foreach (var file in jsonFiles)
            {
                // Read the content of each JSON file
                using var stream = await storage.OpenReadAsync(file);

                // Deserialize the JSON content directly from the stream into a list of cards
                var cards = await JsonSerializer.DeserializeAsync<List<Card>>(stream, new JsonSerializerOptions { }, cancellationToken);

                if (cards != null)
                {
                    _cards.AddRange(cards);
                }
            }
        }

        public Task<Card[]> Search(string query)
        {
            return Task.Run(() => LuceneCardSearchHelper.Filter(_cards.AsQueryable(), query).ToArray());
        }
    }
}
