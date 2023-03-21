#nullable enable
using System.Net.Http;

namespace Mochineko.LLMAgent.Operation
{
    /// <summary>
    /// Pools <see cref="HttpClient"/> to save socket.
    /// </summary>
    internal static class HttpClientPool
    {
        private static HttpClient pooledClient;
        /// <summary>
        /// Pooled <see cref="HttpClient"/>.
        /// </summary>
        public static HttpClient PooledClient => pooledClient;

        static HttpClientPool()
        {
            pooledClient = new HttpClient();
        }

        /// <summary>
        /// Set external <see cref="HttpClient"/> to share instance with other usages.
        /// </summary>
        /// <param name="external"></param>
        /// <param name="disposeOldClient"></param>
        public static void SetExternalClient(HttpClient external, bool disposeOldClient)
        {
            if (disposeOldClient)
            {
                pooledClient?.Dispose();
            }

            pooledClient = external;
        }
    }
}