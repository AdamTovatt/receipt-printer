using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReceiptPrinter.ZettleClasses;
using System.Text;

namespace ReceiptPrinter
{
    public class Receipt : IDocument
    {
        public ReceiptConfig Config { get; set; }
        public string TextContent { get; private set; }
        public string FileName { get; private set; }

        private Receipt(string fileName, string content, ReceiptConfig receiptConfig)
        {
            TextContent = content;
            FileName = fileName;
            Config = receiptConfig;
        }

        public Receipt(Purchase purchase, ReceiptConfig receiptConfig)
        {
            TextContent = GenerateTextContentFromPurchase(purchase, receiptConfig);
            FileName = purchase.PurchaseUuid;
            Config = receiptConfig;
        }

        public static Receipt CreateReceiptFromText(string fileName, string content, ReceiptConfig config) => new Receipt(fileName, content, config);

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.MinSize(new PageSize(Config.PageWidthInMillimeters, Config.PageWidthInMillimeters, Unit.Millimetre));
                page.ContinuousSize(Config.PageWidthInMillimeters, Unit.Millimetre); // Set only the width to 48mm
                page.DefaultTextStyle(x => x.FontSize(Config.FontSize).FontColor(Colors.Black));

                page.Content().Column(column =>
                {
                    column.Item().Text(TextContent).FontSize(Config.FontSize);
                });
            });
        }

        public void GeneratePdf()
        {
            Document.Create(Compose).GeneratePdf($"{FileName}.pdf");
        }

        private string GenerateTextContentFromPurchase(Purchase purchase, ReceiptConfig config)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(purchase.LocalOrderNumber.CenterIn(config.TopDecoration.LoopToLength(config.MaxWidth)));
            result.AppendLine();

            foreach (Product product in purchase.Products)
            {
                if (config.IsForCustomer || product.GetIsInAllowedCategory(config.AllowedCategories))
                    result.AppendLine(product.ToString());
            }

            if (!config.IsForCustomer)
            {
                DateTime time = purchase.Time;
                result.AppendLine();
                result.AppendLine($"{time.ToString("yyyy-MM-dd")} {time.ToShortTimeString()} {purchase.GlobalPurchaseNumber}");
            }

            return result.ToString();
        }
    }
}
