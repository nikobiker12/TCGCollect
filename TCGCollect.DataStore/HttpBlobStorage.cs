using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentStorage;
using FluentStorage.Blobs;

namespace TCGCollect.DataStore
{
    public class HttpBlobStorage : IBlobStorage
    {
        private readonly HttpClient _httpClient;

        public HttpBlobStorage(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<bool>>(fullPaths.Select(f => true).ToArray());
        }

        public Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default)
        {
            return Task<IReadOnlyCollection<Blob>>.FromResult(
                (IReadOnlyCollection<Blob>) new[] { new Blob("sample-data/cards.json", BlobItemKind.File) }
                );
        }

        public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
        {
            return _httpClient.GetStreamAsync(fullPath, cancellationToken);
        }

        public Task<ITransaction> OpenTransactionAsync()
        {
            return Task.FromResult<ITransaction>(null);
        }

        public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}


