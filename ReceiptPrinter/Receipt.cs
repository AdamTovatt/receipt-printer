using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReceiptPrinter.ZettleClasses;

namespace ReceiptPrinter
{
    public class Receipt : IDocument
    {
        private readonly string _content;
        private const float PageWidthInMillimeters = 48f;

        public string PdfFileName { get; private set; }

        private Receipt(string fileName, string content)
        {
            _content = content;
            PdfFileName = "receipt.pdf";
        }

        public Receipt(Purchase purchase)
        {
            _content = purchase.ToString();
            PdfFileName = $"{purchase.PurchaseUuid}.pdf";
        }

        public static Receipt CreateReceiptFromText(string fileName, string content) => new Receipt(fileName, content);

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.ContinuousSize(PageWidthInMillimeters, Unit.Millimetre); // Set only the width to 48mm
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Content().Column(column =>
                {
                    column.Item().Text(_content).FontSize(10);
                });
            });
        }

        public void GeneratePdf(string filePath)
        {
            Document.Create(container =>
            {
                Compose(container);
            }).GeneratePdf(filePath);
        }
    }
}
