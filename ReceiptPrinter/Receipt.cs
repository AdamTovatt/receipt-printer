using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReceiptPrinter.ZettleClasses;

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
            TextContent = purchase.ToString();
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
    }
}
