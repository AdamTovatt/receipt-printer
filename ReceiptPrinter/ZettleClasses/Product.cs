using System.Text.Json.Serialization;

namespace ReceiptPrinter.ZettleClasses
{
    public class Product
    {
        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public int UnitPrice { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("variantName")]
        public string? VariantName { get; set; }

        [JsonPropertyName("category")]
        public ProductCategory? Category { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonConstructor]
        public Product(string quantity, int unitPrice, string name, string? variantName, ProductCategory? category, string? comment)
        {
            Quantity = quantity;
            UnitPrice = unitPrice;
            Name = name;
            VariantName = variantName;
            Category = category;
            Comment = comment;
        }

        public override string ToString()
        {
            string? result;

            if (!string.IsNullOrEmpty(VariantName))
                result = $"{Quantity} x {Name} ({VariantName})";
            else
                result = $"{Quantity} x {Name}";

            if (!string.IsNullOrEmpty(Comment))
                result += $"\nComment: {Comment}\n";

            return result;
        }
    }
}
