using System.Text.Json.Serialization;

namespace ReceiptPrinter.ZettleClasses
{
    public class ZettleAccessToken
    {
        [JsonPropertyName("access_token")]
        public string Token { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonIgnore]
        public DateTime ExpiryTime { get; private set; }

        [JsonIgnore]
        public bool IsExpired => DateTime.Now.AddSeconds(30) > ExpiryTime;

        [JsonConstructor]
        public ZettleAccessToken(string token, int expiresIn)
        {
            Token = token;
            ExpiresIn = expiresIn;

            ExpiryTime = DateTime.Now.AddSeconds(expiresIn);
        }
    }
}
