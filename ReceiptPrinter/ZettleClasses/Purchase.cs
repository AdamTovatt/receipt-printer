using System.Text;
using System.Text.Json.Serialization;

namespace ReceiptPrinter.ZettleClasses
{
    public class Purchase
    {
        [JsonPropertyName("purchaseUUID")]
        public string PurchaseUuid { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("vatAmount")]
        public int VatAmount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("userDisplayName")]
        public string UserDisplayName { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("globalPurchaseNumber")]
        public int GlobalPurchaseNumber { get; set; }

        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }

        [JsonIgnore]
        public string FormattedAmount => $"{Amount / 100.0} {Currency}";

        [JsonConstructor]
        public Purchase(string purchaseUuid, int amount, int vatAmount, string currency, string userDisplayName, string timestamp, int globalPurchaseNumber, List<Product> products)
        {
            Amount = amount;
            PurchaseUuid = purchaseUuid;
            Amount = amount;
            VatAmount = vatAmount;
            Currency = currency;
            UserDisplayName = userDisplayName;
            Products = products;
            Timestamp = timestamp;
            GlobalPurchaseNumber = globalPurchaseNumber;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (Product product in Products)
                stringBuilder.AppendLine(product.ToString());

            DateTime time = DateTime.Parse(Timestamp);
            stringBuilder.AppendLine($"{time.ToShortDateString()} {time.ToShortTimeString()} {GlobalPurchaseNumber}");

            return stringBuilder.ToString();
        }
    }
}
