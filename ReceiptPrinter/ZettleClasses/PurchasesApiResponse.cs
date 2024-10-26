using System.Text.Json.Serialization;

namespace ReceiptPrinter.ZettleClasses
{
    public class PurchasesApiResponse
    {
        [JsonPropertyName("purchases")]
        public List<Purchase> Purchases { get; set; }

        [JsonConstructor]
        public PurchasesApiResponse(List<Purchase> purchases)
        {
            Purchases = purchases;
        }
    }
}
