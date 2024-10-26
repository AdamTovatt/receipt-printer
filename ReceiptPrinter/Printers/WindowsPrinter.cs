namespace ReceiptPrinter.Printers
{
    public class WindowsPrinter : IPrinter
    {
        private ILogger logger;

        public WindowsPrinter(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task PrintAsync(Receipt receipt)
        {
            await Task.CompletedTask;
            receipt.GeneratePdf();
        }
    }
}
