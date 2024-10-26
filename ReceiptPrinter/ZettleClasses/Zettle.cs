using System.Text.Json;

namespace ReceiptPrinter.ZettleClasses
{
    public class Zettle
    {
        private string _clientId;
        private string _clientSecret;

        private HttpClient http;
        private ZettleAccessToken? token;

        public Zettle(ZettleConfig config)
        {
            _clientId = config.ClientId;
            _clientSecret = config.ClientSecret;

            http = new HttpClient();
        }

        public async Task<List<Purchase>> GetPurchasesAsync(int maxResults = 100, bool descending = true, List<string>? allowedCategoryNames = null)
        {
            await EnsureAuthorized();

            string url = $"https://purchase.izettle.com/purchases/v2?descending={descending}&limit={maxResults}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.Token);

            HttpResponseMessage response = await http.SendAsync(request);
            response.EnsureSuccessStatusCodeWithInfo();

            string responseBody = await response.Content.ReadAsStringAsync();

            PurchasesApiResponse? parsedResponse = JsonSerializer.Deserialize<PurchasesApiResponse>(responseBody);

            if (parsedResponse == null)
                throw new Exception("Failed to parse purchases from the response");

            List<Purchase> purchases = parsedResponse.Purchases;

            if (allowedCategoryNames != null)
                purchases = purchases.Where(p => p.Products.Any(x => x.Category != null && allowedCategoryNames.Contains(x.Category.Name.ToLower()))).ToList();

            return purchases;
        }

        public async Task EnsureAuthorized()
        {
            if (token == null || token.IsExpired)
                token = await GetAccessTokenAsync();

            if (token == null)
                throw new Exception("Error when authorizing the application");
        }

        public async Task<ZettleAccessToken> GetAccessTokenAsync()
        {
            const string url = "https://oauth.zettle.com/token";
            FormUrlEncodedContent requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("assertion", _clientSecret)
            });

            HttpResponseMessage response = await http.PostAsync(url, requestData);
            response.EnsureSuccessStatusCodeWithInfo();

            string responseBody = await response.Content.ReadAsStringAsync();
            ZettleAccessToken? accessToken = JsonSerializer.Deserialize<ZettleAccessToken>(responseBody);

            if (accessToken == null)
                throw new Exception("Failed to parse the access token");

            return accessToken;
        }
    }
}
