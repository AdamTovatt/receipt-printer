namespace ReceiptPrinter.Printers
{
    public class WindowsPrinter : IPrinter
    {
        public void Print(Receipt receipt)
        {
            receipt.GeneratePdf(receipt.PdfFileName);
        }
    }
}
