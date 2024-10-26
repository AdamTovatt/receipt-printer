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

        public string FileName { get; private set; }

        private Receipt(string fileName, string content)
        {
            _content = content;
            FileName = fileName;
        }

        public Receipt(Purchase purchase)
        {
            _content = purchase.ToString();
            FileName = purchase.PurchaseUuid;
        }

        public static Receipt CreateReceiptFromText(string fileName, string content) => new Receipt(fileName, content);

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.MinSize(new PageSize(PageWidthInMillimeters, PageWidthInMillimeters, Unit.Millimetre));
                page.ContinuousSize(PageWidthInMillimeters, Unit.Millimetre); // Set only the width to 48mm
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Content().Column(column =>
                {
                    column.Item().Text(_content).FontSize(10);
                });
            });
        }

        public void GeneratePdf()
        {
            Document.Create(Compose).GeneratePdf($"{FileName}.pdf");
        }

        public void GenerateTxtFile(int? maxCharactersWide = null)
        {
            string? writeContent = null;

            if (maxCharactersWide.HasValue)
            {
                writeContent = _content.SplitToLines(maxCharactersWide.Value);
            }
            else
            {
                writeContent = _content;
            }

            File.WriteAllText($"{FileName}.txt", writeContent);
        }

        public void RemoveTextFile()
        {
            File.Delete($"{FileName}.txt");
        }
    }
}
