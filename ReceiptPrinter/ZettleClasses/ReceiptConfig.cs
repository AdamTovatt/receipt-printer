namespace ReceiptPrinter.ZettleClasses
{
    public class ReceiptConfig
    {
        public List<string>? AllowedCategories { get; set; }

        /// <summary>
        /// Comma separated
        /// </summary>
        /// <param name="allowedCategories"></param>
        public ReceiptConfig(string? allowedCategories)
        {
            List<string>? categories = allowedCategories?.Split(',').ToList();

            if (categories != null)
            {
                AllowedCategories = new List<string>();

                foreach (string category in categories)
                    AllowedCategories.Add(category.ToLower());
            }
        }
    }
}
