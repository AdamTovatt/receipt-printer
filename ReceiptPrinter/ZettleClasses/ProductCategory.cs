using System.Text.Json.Serialization;

namespace ReceiptPrinter.ZettleClasses
{
    public class ProductCategory
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonConstructor]
        public ProductCategory(string uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }
}
