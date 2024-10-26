namespace ReceiptPrinter.ZettleClasses
{
    public class ZettleConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public ZettleConfig(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
