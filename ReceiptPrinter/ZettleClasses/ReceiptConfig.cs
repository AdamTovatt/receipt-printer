namespace ReceiptPrinter.ZettleClasses
{
    public class ReceiptConfig
    {
        public List<string>? AllowedCategories { get; set; }
        public int FontSize { get; set; } = 10; // default to 10
        public float PageWidthInMillimeters { get; set; } = 48f; // default to 48 mm
        public bool IsForCustomer { get; set; } = false;
        public int BottomMargin { get; set; } = 1;
        public string BottomDecoration { get; set; } = "~";
        public string TopDecoration { get; set; } = "~";

        private int maxWidth;
        private int cpi;
        private int lpi;

        /// <summary>
        /// The max amount of characters that can fit on one line
        /// </summary>
        public int MaxWidth => maxWidth;
        public int Cpi => cpi;
        public int Lpi => lpi;

        /// <summary>
        /// Comma separated
        /// </summary>
        /// <param name="allowedCategories"></param>
        public ReceiptConfig(
            string? allowedCategories = null,
            int? fontSize = null,
            float? pageWidth = null,
            bool? isForCustomer = null,
            int? bottomMargin = null,
            string? bottomDecoration = null,
            string? topDecoration = null)
        {
            List<string>? categories = allowedCategories?.Split(',').ToList();

            if (categories != null)
            {
                AllowedCategories = new List<string>();

                foreach (string category in categories)
                    AllowedCategories.Add(category.ToLower());
            }

            if (fontSize != null)
                FontSize = fontSize.Value;

            if (pageWidth != null)
                PageWidthInMillimeters = pageWidth.Value;

            if (isForCustomer != null)
                IsForCustomer = isForCustomer.Value;

            if (bottomMargin != null)
                BottomMargin = bottomMargin.Value;

            if (bottomDecoration != null)
                BottomDecoration = bottomDecoration;

            if (topDecoration != null)
                TopDecoration = topDecoration;

            // Calculate and set MaxWidth, Cpi, and Lpi
            cpi = Math.Clamp(24 - FontSize, 4, 23);
            maxWidth = (int)Math.Round(PageWidthInMillimeters / 25.4 * cpi);
            lpi = (int)Math.Round(cpi / 1.75);
        }
    }
}
