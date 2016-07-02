using Mirror.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading.Tasks;


namespace Mirror.Networking
{
    class ApiClient
    {
        static JsonSerializerSettings Settings => new JsonSerializerSettings()
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        internal static Task<T> GetAsync<T>(string url, Func<HttpClient> getClient = null) => 
            Do.WithRetry(() => GetAsyncImpl<T>(url, getClient));

        static async Task<T> GetAsyncImpl<T>(string url, Func<HttpClient> getClient = null)
        {
            T result = default(T);
            getClient = getClient ?? (() => new HttpClient());
            var response = await GetRawAsync(url, getClient);
            if (!string.IsNullOrWhiteSpace(response))
            {
                result = JsonConvert.DeserializeObject<T>(response, Settings);
            }
            return result;
        }

        internal static async Task<string> GetRawAsync(string url, Func<HttpClient> getClient)
        {
            Contract.Assert(url != null);
            Contract.Assert(getClient != null);
            
            using (var client = getClient?.Invoke())
            {
                return await client.GetStringAsync(url);
            }
        }
    }
}