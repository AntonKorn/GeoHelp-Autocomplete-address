namespace DataLoader.Services
{
    public class OverpassApiService
    {
        public async Task<string> QueryAll(string overpassQuery)
        {
            var client = CreateClient();

            var request = new HttpRequestMessage()
            {
                Content = new FormUrlEncodedContent(BuildOverpassRequest(overpassQuery))
            };

            var response = client.Send(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to request overpass api");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://overpass-api.de/api/interpreter");
            return client;
        }

        private IDictionary<string, string> BuildOverpassRequest(string query)
        {
            return new Dictionary<string, string>()
            {
                { "data", query }
            };
        }
    }
}
